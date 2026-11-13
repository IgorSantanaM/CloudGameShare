using CloudGameShare.GamingSession.Domain.Sessions;
using k8s.Models;

namespace CloudGameShare.GamingSession.Infra.Services.Provisioning.KubernetesSession
{
    public interface IKubernetesSessionProvisioner
    {
        public Task<PodState> ObserveAsync(GameSession session, CancellationToken cancellationToken);
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    }
}
