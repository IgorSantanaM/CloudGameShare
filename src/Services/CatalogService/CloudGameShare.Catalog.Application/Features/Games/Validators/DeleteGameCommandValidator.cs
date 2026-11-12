using CloudGameShare.Catalog.Application.Features.Games.Commands.DeleteGame;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

public sealed class DeleteGameCommandValidator : AbstractValidator<DeleteGameCommand>
{
    public DeleteGameCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Game ID is required.");
    }
}
