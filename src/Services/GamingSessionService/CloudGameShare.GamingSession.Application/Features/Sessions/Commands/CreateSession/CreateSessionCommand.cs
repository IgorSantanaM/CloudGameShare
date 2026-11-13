using CloudGameShare.GamingSession.Application.Features.Sessions.DTOs;
using MediatR;

namespace CloudGameShare.GamingSession.Application.Features.Sessions.Commands.CreateSession;

public sealed record CreateSessionCommand(string OwnerId, Guid GameId, Guid? IdempotencyKey) : IRequest<SessionStatusDto>;
