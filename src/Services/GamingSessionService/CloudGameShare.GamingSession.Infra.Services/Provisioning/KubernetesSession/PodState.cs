namespace CloudGameShare.GamingSession.Infra.Services.Provisioning.KubernetesSession;

public enum PodState
{ 
    Missing,
    Waiting, 
    Ready, 
    Failed, 
    Completed, 
    NameConflict 
}
