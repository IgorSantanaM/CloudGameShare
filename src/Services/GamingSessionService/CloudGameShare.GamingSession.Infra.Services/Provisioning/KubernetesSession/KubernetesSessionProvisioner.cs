using System.Globalization;
using System.Net;
using CloudGameShare.GamingSession.Domain.Sessions;
using k8s;
using k8s.Autorest;
using k8s.Models;

namespace CloudGameShare.GamingSession.Infra.Services.Provisioning.KubernetesSession;

internal sealed class KubernetesSessionProvisioner(IKubernetes kubernetes, ProvisioningOptions options) : IKubernetesSessionProvisioner
{
    private const string SessionLabel = "cloudgameshare.io/session-id";
    private const string ManagedByLabel = "app.kubernetes.io/managed-by";
    private const string Manager = "cloudgameshare-session";

    public async Task<PodState> ObserveAsync(GameSession session, CancellationToken cancellationToken)
    {
        var pod = await ReadAsync(session.Id, cancellationToken);
        if (pod is null && session.ReadyAtUtc is null)
        {
            try
            {
                pod = await kubernetes.CoreV1.CreateNamespacedPodAsync(CreatePod(session), options.Namespace,
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

    private V1Pod CreatePod(GameSession session)
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
            new() { Name = "GAME_RUNTIME", Value = game.Runtime == SessionRuntime.Proton ? "proton" : "linux-native" }
        };
        if (game.ProtonVersion is { } protonVersion)
            environment.Add(new V1EnvVar { Name = "PROTON_VERSION", Value = protonVersion });

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
                Containers = [
                    new V1Container
                    {
                        Name = "game",
                        Image = options.Image,
                        Env = environment,
                        Resources = new V1ResourceRequirements { Requests = resources, Limits = resources },
                        SecurityContext = new V1SecurityContext
                        {
                            AllowPrivilegeEscalation = false,
                            Capabilities = new V1Capabilities { Drop = ["ALL"] }
                        },
                        ReadinessProbe = new V1Probe
                        {
                            HttpGet = new V1HTTPGetAction { Path = options.ReadinessPath, Port = options.ReadinessPort },
                            PeriodSeconds = 5
                        }
                    }
                ]
            }
        };
    }

    private static string PodName(Guid id) => $"game-{id:N}";

    private static bool OwnsPod(V1Pod pod, Guid id)
        => pod.Metadata?.Labels is { } labels
            && labels.TryGetValue(ManagedByLabel, out var manager) && manager == Manager
            && labels.TryGetValue(SessionLabel, out var sessionId) && sessionId == id.ToString("N");
}
