using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CloudGameShare.GamingSession.Domain.Sessions;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace CloudGameShare.GamingSession.Infra.Services.Provisioning.KubernetesSession;

internal sealed class KubernetesSessionProvisioner(IKubernetes kubernetes, ProvisioningOptions options) : IKubernetesSessionProvisioner
{
    private const string SessionLabel = "cloudgameshare.io/session-id";
    private const string ManagedByLabel = "app.kubernetes.io/managed-by";
    private const string PlayerStorageAnnotation = "cloudgameshare.io/player-game-storage";
    private const string Manager = "cloudgameshare-session";

    public async Task<PodState> ObserveAsync(GameSession session, CancellationToken cancellationToken)
    {
        var pod = await ReadAsync(session.Id, cancellationToken);
        if (pod is null && session.ReadyAtUtc is null)
        {
            var playerClaim = options.PlayerStorageGiB is int storageGiB
                ? await EnsurePlayerClaimAsync(session, storageGiB, cancellationToken)
                : null;

            try
            {
                pod = await kubernetes.CoreV1.CreateNamespacedPodAsync(CreatePod(session, playerClaim), options.Namespace,
                    cancellationToken: cancellationToken);
            }
            catch (HttpOperationException exception) when (exception.Response.StatusCode == HttpStatusCode.Conflict)
            {
                pod = await ReadAsync(session.Id, cancellationToken);
            }
        }

        if (pod is null)
            return PodState.Missing;

        if (!OwnsPod(pod, session.Id))
            return PodState.NameConflict;

        return pod.Status?.Phase switch
        {
            "Failed" => PodState.Failed,
            "Succeeded" => PodState.Completed,
            "Running" when pod.Status.Conditions?.Any(condition => condition.Type == "Ready" && condition.Status == "True") == true
                => PodState.Ready,
            _ => PodState.Waiting
        };
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var pod = await ReadAsync(id, cancellationToken);
        if (pod is null || !OwnsPod(pod, id))
            return;

        try
        {
            var deleteOptions = new V1DeleteOptions { Preconditions = new V1Preconditions { Uid = pod.Metadata.Uid } };
            await kubernetes.CoreV1.DeleteNamespacedPodAsync(PodName(id), options.Namespace, deleteOptions,
                cancellationToken: cancellationToken);
        }
        catch (HttpOperationException exception) when (exception.Response.StatusCode == HttpStatusCode.NotFound)
        {
        }
    }

    private async Task<V1Pod?> ReadAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await kubernetes.CoreV1.ReadNamespacedPodAsync(PodName(id), options.Namespace,
                cancellationToken: cancellationToken);
        }
        catch (HttpOperationException exception) when (exception.Response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private async Task<string> EnsurePlayerClaimAsync(GameSession session, int storageGiB, CancellationToken cancellationToken)
    {
        var storageKey = Convert.ToHexStringLower(SHA256.HashData(
            Encoding.UTF8.GetBytes($"{session.OwnerId}:{session.GameId:N}")));
        var name = $"player-{storageKey[..56]}";
        V1PersistentVolumeClaim claim;

        try
        {
            claim = await kubernetes.CoreV1.ReadNamespacedPersistentVolumeClaimAsync(name, options.Namespace,
                cancellationToken: cancellationToken);
        }
        catch (HttpOperationException exception) when (exception.Response.StatusCode == HttpStatusCode.NotFound)
        {
            var requested = new V1PersistentVolumeClaim
            {
                Metadata = new V1ObjectMeta
                {
                    Name = name,
                    Labels = new Dictionary<string, string> { [ManagedByLabel] = Manager },
                    Annotations = new Dictionary<string, string> { [PlayerStorageAnnotation] = storageKey }
                },
                Spec = new V1PersistentVolumeClaimSpec
                {
                    StorageClassName = options.PlayerStorageClass,
                    AccessModes = ["ReadWriteOncePod"],
                    Resources = new V1VolumeResourceRequirements
                    {
                        Requests = new Dictionary<string, ResourceQuantity>
                        {
                            ["storage"] = new(FormattableString.Invariant($"{storageGiB}Gi"))
                        }
                    }
                }
            };

            try
            {
                claim = await kubernetes.CoreV1.CreateNamespacedPersistentVolumeClaimAsync(requested, options.Namespace,
                    cancellationToken: cancellationToken);
            }
            catch (HttpOperationException conflict) when (conflict.Response.StatusCode == HttpStatusCode.Conflict)
            {
                claim = await kubernetes.CoreV1.ReadNamespacedPersistentVolumeClaimAsync(name, options.Namespace,
                    cancellationToken: cancellationToken);
            }
        }

        if (claim.Metadata?.DeletionTimestamp is not null
            || claim.Metadata?.Annotations?.TryGetValue(PlayerStorageAnnotation, out var ownerKey) != true
            || ownerKey != storageKey
            || claim.Spec?.AccessModes?.Contains("ReadWriteOncePod") != true)
            throw new InvalidOperationException($"Player storage claim '{name}' is unavailable.");

        return name;
    }

    private V1Pod CreatePod(GameSession session, string? playerClaim)
    {
        var game = session.LaunchSpec;
        var resources = new Dictionary<string, ResourceQuantity>
        {
            ["cpu"] = new(game.CpuCores.ToString(CultureInfo.InvariantCulture)),
            ["memory"] = new(FormattableString.Invariant($"{game.MemoryGb}Gi")),
            ["ephemeral-storage"] = new(FormattableString.Invariant($"{game.StorageGb}Gi")),
            [options.GpuResourceName] = new(game.GpuCount.ToString(CultureInfo.InvariantCulture))
        };
        var environment = new List<V1EnvVar>
        {
            new() { Name = "GAMING_SESSION_ID", Value = session.Id.ToString("D") },
            new() { Name = "STEAM_APP_ID", Value = game.SteamAppId },
            new() { Name = "GAME_RUNTIME", Value = game.Runtime == SessionRuntime.Proton ? "proton" : "linux-native" },
            new() { Name = "SELKIES_BASIC_AUTH_USER", Value = "ubuntu" },
            CreateSelkiesPassword()
        };
        if (game.ProtonVersion is { } protonVersion)
            environment.Add(new V1EnvVar { Name = "PROTON_VERSION", Value = protonVersion });

        var volumes = new List<V1Volume>
        {
            new() { Name = "shared-memory", EmptyDir = new V1EmptyDirVolumeSource { Medium = "Memory", SizeLimit = new("2Gi") } }
        };
        var mounts = new List<V1VolumeMount>
        {
            new() { Name = "shared-memory", MountPath = "/dev/shm" }
        };
        if (playerClaim is not null)
        {
            AddClaim("player-home", playerClaim, "/home/ubuntu", readOnly: false);
            environment.Add(new V1EnvVar { Name = "GAME_STATE_DIR", Value = "/home/ubuntu/.game-state" });
        }

        if (!string.IsNullOrWhiteSpace(options.GameLibraryClaim))
            AddClaim("game-library", options.GameLibraryClaim, "/steam-library", readOnly: true);

        return new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta
            {
                Name = PodName(session.Id),
                Labels = new Dictionary<string, string> { [SessionLabel] = session.Id.ToString("N"), [ManagedByLabel] = Manager }
            },
            Spec = new V1PodSpec
            {
                RestartPolicy = "Never",
                ActiveDeadlineSeconds = options.SessionLifetimeSeconds,
                AutomountServiceAccountToken = false,
                SecurityContext = playerClaim is null ? null : new V1PodSecurityContext { FsGroup = 1000, FsGroupChangePolicy = "OnRootMismatch" },
                Volumes = volumes,
                Containers = [
                    new V1Container
                    {
                        Name = "game",
                        Image = options.Image,
                        Env = environment,
                        Ports = [
                            new V1ContainerPort { Name = "selkies", ContainerPort = 8080 },
                            new V1ContainerPort { Name = "readiness", ContainerPort = options.ReadinessPort }
                        ],
                        VolumeMounts = mounts,
                        Resources = new V1ResourceRequirements { Requests = resources, Limits = resources },
                        SecurityContext = new V1SecurityContext
                        {
                            AllowPrivilegeEscalation = false,
                            Capabilities = new V1Capabilities { Drop = ["ALL"] },
                            SeccompProfile = CreateSeccompProfile()
                        },
                        ReadinessProbe = new V1Probe
                        {
                            HttpGet = new V1HTTPGetAction { Path = options.ReadinessPath, Port = options.ReadinessPort },
                            PeriodSeconds = 5,
                            TimeoutSeconds = 5
                        }
                    }
                ]
            }
        };

        void AddClaim(string name, string claimName, string mountPath, bool readOnly)
        {
            volumes.Add(new V1Volume
            {
                Name = name,
                PersistentVolumeClaim = new V1PersistentVolumeClaimVolumeSource { ClaimName = claimName, ReadOnlyProperty = readOnly }
            });
            mounts.Add(new V1VolumeMount { Name = name, MountPath = mountPath, ReadOnlyProperty = readOnly });
        }
    }

    private V1EnvVar CreateSelkiesPassword()
    {
        const string passwordVariable = "SELKIES_BASIC_AUTH_PASSWORD";
        if (string.IsNullOrWhiteSpace(options.SelkiesPasswordSecret))
            return new V1EnvVar { Name = passwordVariable, Value = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(24)) };

        return new V1EnvVar
        {
            Name = passwordVariable,
            ValueFrom = new V1EnvVarSource
            {
                SecretKeyRef = new V1SecretKeySelector { Name = options.SelkiesPasswordSecret, Key = "password" }
            }
        };
    }

    private V1SeccompProfile CreateSeccompProfile()
    {
        if (options.AllowUnconfinedSeccomp)
            return new V1SeccompProfile { Type = "Unconfined" };

        return string.IsNullOrWhiteSpace(options.SeccompLocalhostProfile)
            ? new V1SeccompProfile { Type = "RuntimeDefault" }
            : new V1SeccompProfile { Type = "Localhost", LocalhostProfile = options.SeccompLocalhostProfile };
    }

    private static string PodName(Guid id) => $"game-{id:N}";

    private static bool OwnsPod(V1Pod pod, Guid id)
        => pod.Metadata?.Labels is { } labels
            && labels.TryGetValue(ManagedByLabel, out var manager) && manager == Manager
            && labels.TryGetValue(SessionLabel, out var sessionId) && sessionId == id.ToString("N");
}
