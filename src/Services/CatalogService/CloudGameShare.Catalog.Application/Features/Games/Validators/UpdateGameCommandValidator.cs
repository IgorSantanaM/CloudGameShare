using CloudGameShare.Catalog.Application.Features.Games.Commands.UpdateGame;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

public sealed class UpdateGameCommandValidator : AbstractValidator<UpdateGameCommand>
{
    public UpdateGameCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty().WithMessage("Game ID is required.");
        RuleFor(command => command.Name).NotEmpty().WithMessage("Game name is required.");
        RuleFor(command => command.Genre).NotEmpty().WithMessage("Game genre is required.");
        RuleFor(command => command.Publisher).NotEmpty().WithMessage("Game publisher is required.");
        RuleFor(command => command.Developer).NotEmpty().WithMessage("Game developer is required.");
        RuleFor(command => command.Requirements)
            .NotNull().WithMessage("Game requirements are required.")
            .SetValidator(new GameRequirementsDtoValidator());
        RuleFor(command => command.LaunchConfig)
            .NotNull().WithMessage("Game launch configuration is required.")
            .SetValidator(new GameLaunchConfigDtoValidator());
    }
}
