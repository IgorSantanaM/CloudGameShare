namespace CloudGameShare.GamingSession.Domain.Sessions;

public sealed record SessionLaunchSpec(
    string SteamAppId,
    SessionRuntime Runtime,
    string? ProtonVersion,
    int CpuCores,
    int MemoryGb,
    int GpuCount,
    int StorageGb);

public enum SessionRuntime
{
    LinuxNative,
    Proton
}
