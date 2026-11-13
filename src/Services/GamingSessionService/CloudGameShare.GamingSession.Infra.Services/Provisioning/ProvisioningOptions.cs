namespace CloudGameShare.GamingSession.Infra.Services.Provisioning;

internal sealed class ProvisioningOptions
{
    public string? Image { get; set; }
    public string Namespace { get; set; } = "GameShare";
    public string? KubeContext { get; set; }
    public string? KubeConfigPath { get; set; }
    public string GpuResourceName { get; set; } = "nvidia.com/gpu";
    public int ReadinessPort { get; set; } = 8080;
    public string ReadinessPath { get; set; } = "/ready";
    public int TimeoutSeconds { get; set; } = 600;
    public int SessionLifetimeSeconds { get; set; } = 7200;
}
