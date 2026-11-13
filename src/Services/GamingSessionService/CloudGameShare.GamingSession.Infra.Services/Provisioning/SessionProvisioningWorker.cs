using CloudGameShare.GamingSession.Domain.Sessions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CloudGameShare.GamingSession.Infra.Services.Provisioning;

internal sealed class SessionProvisioningWorker(
    IGamingSessionRepository repository,
    KubernetesSessionProvisioner provisioner,
    TimeProvider timeProvider,
    ILogger<SessionProvisioningWorker> logger) : BackgroundService
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var session = await repository.ClaimNextAsync(UtcNow(), LeaseDuration, stoppingToken);
                if (session is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    continue;
                }

                await ProcessAsync(session, stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "Could not process the session queue. The worker will retry.");
                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task ProcessAsync(GameSession session, CancellationToken stoppingToken)
    {
        using var operation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        operation.CancelAfter(OperationTimeout);
        var now = UtcNow();
        GameSession updated;
        try
        {
            updated = await AdvanceAsync(session, now, operation.Token);
        }
        catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Pod operation for session {SessionId} will be retried.", session.Id);
            updated = session with { NextActionAtUtc = UtcNow().Add(RetryDelay) };
        }

        await repository.CompleteClaimAsync(updated, stoppingToken);
    }

    private async Task<GameSession> AdvanceAsync(GameSession session, DateTime now, CancellationToken cancellationToken)
    {
        if (session.Status is SessionStatus.Ended or SessionStatus.Failed)
        {
            await provisioner.DeleteAsync(session.Id, cancellationToken);
            return session with { NextActionAtUtc = null };
        }

        if (session.ReadyAtUtc is null && now >= session.ProvisionByUtc)
            return session.Finish(now, "The game did not become ready before the provisioning timeout.");
        if (now >= session.ExpiresAtUtc)
            return session.Finish(now);

        var pod = await provisioner.ObserveAsync(session, cancellationToken);
        return pod switch
        {
            PodState.Failed => session.Finish(now, "The game pod failed."),
            PodState.NameConflict => session.Finish(now, "The game pod name is already in use."),
            PodState.Completed or PodState.Missing when session.ReadyAtUtc is not null => session.Finish(now),
            PodState.Completed => session.Finish(now, "The game exited before it became ready."),
            PodState.Ready => session.Schedule(SessionStatus.Ready, now.Add(RetryDelay), now),
            _ => session.Schedule(SessionStatus.Provisioning, now.Add(RetryDelay), now)
        };
    }

    private DateTime UtcNow() => timeProvider.GetUtcNow().UtcDateTime;
}
