using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

internal sealed class GameRequirementsDtoValidator : AbstractValidator<GameRequirementsDto>
{
    public GameRequirementsDtoValidator()
    {
        RuleFor(requirements => requirements.CpuCores).GreaterThan(0).WithMessage("CPU cores must be greater than zero.");
        RuleFor(requirements => requirements.MemoryGb).GreaterThan(0).WithMessage("Memory in GB must be greater than zero.");
        RuleFor(requirements => requirements.GpuCount).GreaterThan(0).WithMessage("GPU count must be greater than zero.");
        RuleFor(requirements => requirements.StorageGb).GreaterThan(0).WithMessage("Storage in GB must be greater than zero.");
    }
}
