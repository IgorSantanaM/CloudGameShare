using CloudGameShare.Catalog.Application.Features.Games.Commands.UnpublishGame;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

public sealed class UnpublishGameCommandValidator : AbstractValidator<UnpublishGameCommand>
{
    public UnpublishGameCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Game ID is required.");
    }
}
