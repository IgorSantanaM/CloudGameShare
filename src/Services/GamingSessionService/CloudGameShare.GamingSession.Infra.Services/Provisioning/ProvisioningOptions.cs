namespace CloudGameShare.GamingSession.Infra.Services.Provisioning;

internal sealed class ProvisioningOptions
{
    public const string DefaultImage = "cloudgameshare/steam-selkies:dev";

    public bool? Enabled { get; set; }
    public string? Image { get; set; }
    public string Namespace { get; set; } = "default";
    public string? KubeContext { get; set; }
    public string? KubeConfigPath { get; set; }
    public string GpuResourceName { get; set; } = "nvidia.com/gpu";
    public int ReadinessPort { get; set; } = 8081;
    public string ReadinessPath { get; set; } = "/ready";
    public string? PlayerStorageClass { get; set; }
    public int? PlayerStorageGiB { get; set; }
    public string? GameLibraryClaim { get; set; }
    public string? SelkiesPasswordSecret { get; set; }
    public string? SeccompLocalhostProfile { get; set; }
    public bool AllowUnconfinedSeccomp { get; set; }
    public int TimeoutSeconds { get; set; } = 600;
    public int SessionLifetimeSeconds { get; set; } = 7200;
}
