using CloudGameShare.GamingSession.Application.Features.Sessions.Commands.CreateSession;
using FluentValidation;

namespace CloudGameShare.GamingSession.Application.Features.Sessions.Validators;

public sealed class CreateSessionCommandValidator : AbstractValidator<CreateSessionCommand>
{
    public CreateSessionCommandValidator()
    {
        RuleFor(command => command.OwnerId).NotEmpty().WithMessage("An authenticated player is required.");
        RuleFor(command => command.GameId).NotEmpty().WithMessage("Game ID is required.");
        RuleFor(command => command.IdempotencyKey).NotEqual(Guid.Empty).WithMessage("Idempotency key must be a nonempty GUID.");
    }
}
