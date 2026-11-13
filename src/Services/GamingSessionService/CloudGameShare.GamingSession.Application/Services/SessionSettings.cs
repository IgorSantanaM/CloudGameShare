namespace CloudGameShare.GamingSession.Application.Services;

public sealed record SessionSettings(bool ProvisioningEnabled, TimeSpan ProvisioningTimeout, TimeSpan SessionLifetime);
