using CloudGameShare.Catalog.Contracts;
using CloudGameShare.GamingSession.Application.Services;
using CloudGameShare.GamingSession.Infra.Services.Catalog;
using CloudGameShare.GamingSession.Infra.Services.Provisioning;
using CloudGameShare.GamingSession.Infra.Services.Provisioning.KubernetesSession;
using k8s;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudGameShare.GamingSession.Infra.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddGamingSessionServices(this IServiceCollection services, IConfiguration configuration)
    {
        var catalogAddress = configuration["Catalog:GrpcAddress"]
            ?? throw new InvalidOperationException("Configure Catalog:GrpcAddress.");

        services.AddGrpcClient<CatalogSessions.CatalogSessionsClient>(options => options.Address = new Uri(catalogAddress));

        services.AddScoped<ICatalogSessionClient, GrpcCatalogSessionClient>();

        var provisioning = configuration.GetSection("Provisioning").Get<ProvisioningOptions>() ?? new ProvisioningOptions();

        var enabled = provisioning.Enabled ?? !string.IsNullOrWhiteSpace(provisioning.Image);

        if (provisioning.TimeoutSeconds <= 0 || provisioning.SessionLifetimeSeconds < provisioning.TimeoutSeconds)
            throw new InvalidOperationException("Provisioning timeout must be positive and cannot exceed session lifetime.");

        services.AddSingleton(new SessionSettings(enabled, TimeSpan.FromSeconds(provisioning.TimeoutSeconds),
            TimeSpan.FromSeconds(provisioning.SessionLifetimeSeconds)));

        if (!enabled)
            return services;

        if (string.IsNullOrWhiteSpace(provisioning.Image))
            provisioning.Image = ProvisioningOptions.DefaultImage;

        if (string.IsNullOrWhiteSpace(provisioning.Namespace) || string.IsNullOrWhiteSpace(provisioning.GpuResourceName)
            || provisioning.ReadinessPort is < 1 or > 65535 || !provisioning.ReadinessPath.StartsWith('/'))
            throw new InvalidOperationException("Configure the provisioning namespace, GPU resource, and HTTP readiness probe.");

        if (provisioning.PlayerStorageGiB is <= 0
            || provisioning.PlayerStorageGiB is null && !string.IsNullOrWhiteSpace(provisioning.PlayerStorageClass))
            throw new InvalidOperationException("Set Provisioning:PlayerStorageGiB to a positive size to enable player storage.");

        if (provisioning.AllowUnconfinedSeccomp && !string.IsNullOrWhiteSpace(provisioning.SeccompLocalhostProfile))
            throw new InvalidOperationException("Choose either a local seccomp profile or unconfined seccomp.");

        services.AddSingleton(provisioning);
        services.AddSingleton<IKubernetes>(_ => new Kubernetes(CreateKubeConfig(provisioning)));

        services.AddSingleton<IKubernetesSessionProvisioner, KubernetesSessionProvisioner>();

        services.AddHostedService<SessionProvisioningWorker>();

        return services;
    }

    private static KubernetesClientConfiguration CreateKubeConfig(ProvisioningOptions provisioning)
    {
        if (KubernetesClientConfiguration.IsInCluster())
            return KubernetesClientConfiguration.InClusterConfig();

        if (string.IsNullOrWhiteSpace(provisioning.KubeContext))
            throw new InvalidOperationException("Configure Provisioning:KubeContext when provisioning outside Kubernetes.");

        return KubernetesClientConfiguration.BuildConfigFromConfigFile(
            provisioning.KubeConfigPath ?? KubernetesClientConfiguration.KubeConfigDefaultLocation,
            provisioning.KubeContext);
    }
}
