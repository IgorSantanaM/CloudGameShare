using CloudGameShare.Catalog.Application.Features.Games.Commands.PublishGame;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

public sealed class PublishGameCommandValidator : AbstractValidator<PublishGameCommand>
{
    public PublishGameCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Game ID is required.");
    }
}
