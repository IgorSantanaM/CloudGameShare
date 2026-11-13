namespace CloudGameShare.GamingSession.Application.Exceptions;

public sealed class SessionRequestException(SessionRequestError error, string message) : Exception(message)
{
    public SessionRequestError Error { get; } = error;
}

public enum SessionRequestError
{
    NotFound,
    Conflict,
    Unavailable
}
