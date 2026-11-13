namespace CloudGameShare.GamingSession.Domain.Sessions;

public sealed record GameSession(
    Guid Id,
    string OwnerId,
    Guid GameId,
    SessionLaunchSpec LaunchSpec,
    DateTime CreatedAtUtc,
    DateTime ProvisionByUtc,
    DateTime ExpiresAtUtc)
{
    public SessionStatus Status { get; init; } = SessionStatus.Pending;
    public DateTime? NextActionAtUtc { get; init; } = CreatedAtUtc;
    public DateTime? ReadyAtUtc { get; init; }
    public DateTime? EndedAtUtc { get; init; }
    public string? FailureReason { get; init; }
    public string? LeaseId { get; init; }
    public DateTime? LeaseUntilUtc { get; init; }

    public GameSession Schedule(SessionStatus status, DateTime nextActionAtUtc, DateTime now)
        => this with
        {
            Status = status,
            NextActionAtUtc = nextActionAtUtc,
            ReadyAtUtc = status == SessionStatus.Ready ? ReadyAtUtc ?? now : ReadyAtUtc
        };

    public GameSession Finish(DateTime now, string? failureReason = null)
        => this with
        {
            Status = failureReason is null ? SessionStatus.Ended : SessionStatus.Failed,
            EndedAtUtc = now,
            FailureReason = failureReason,
            NextActionAtUtc = now
        };
}
