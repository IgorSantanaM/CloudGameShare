using CloudGameShare.GamingSession.Application.Features.Sessions.DTOs;
using MediatR;

namespace CloudGameShare.GamingSession.Application.Features.Sessions.Queries.GetSessionStatus;

public sealed record GetSessionStatusQuery(string OwnerId, Guid Id) : IRequest<SessionStatusDto>;
