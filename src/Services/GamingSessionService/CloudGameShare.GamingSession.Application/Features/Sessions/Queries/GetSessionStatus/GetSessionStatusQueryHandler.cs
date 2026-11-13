using CloudGameShare.GamingSession.Application.Exceptions;
using CloudGameShare.GamingSession.Application.Features.Sessions.DTOs;
using CloudGameShare.GamingSession.Domain.Sessions;
using MediatR;

namespace CloudGameShare.GamingSession.Application.Features.Sessions.Queries.GetSessionStatus;

public sealed class GetSessionStatusQueryHandler(IGamingSessionRepository repository)
    : IRequestHandler<GetSessionStatusQuery, SessionStatusDto>
{
    public async Task<SessionStatusDto> Handle(GetSessionStatusQuery request, CancellationToken cancellationToken)
    {
        var session = await repository.GetByIdAsync(request.Id, request.OwnerId, cancellationToken)
            ?? throw new SessionRequestException(SessionRequestError.NotFound, "Gaming session was not found.");

        return SessionStatusDto.From(session);
    }
}
