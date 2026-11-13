namespace CloudGameShare.GamingSession.Domain.Sessions;

public enum SessionStatus
{
    Pending,
    Provisioning,
    Ready,
    Failed,
    Ended
}
