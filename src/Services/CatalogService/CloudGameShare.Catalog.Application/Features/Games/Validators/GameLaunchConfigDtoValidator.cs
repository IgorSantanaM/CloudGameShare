using System.Globalization;
using CloudGameShare.Catalog.Application.Features.Games.DTOs;
using CloudGameShare.Catalog.Domain.Games.Enum;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

internal sealed class GameLaunchConfigDtoValidator : AbstractValidator<GameLaunchConfigDto>
{
    public GameLaunchConfigDtoValidator()
    {
        RuleFor(config => config.Launcher).IsInEnum().WithMessage("Select a valid game launcher.");
        RuleFor(config => config.Runtime).IsInEnum().WithMessage("Select a valid game runtime.");

        RuleFor(config => config.AppId)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Game App ID is required.")
            .Must(appId => uint.TryParse(appId.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var steamId)
                && steamId > 0)
            .When(config => config.Launcher == GameLauncher.Steam, ApplyConditionTo.CurrentValidator)
            .WithMessage("Steam App ID must be a positive numeric identifier.");

        RuleFor(config => config.ProtonVersion)
            .Must(string.IsNullOrWhiteSpace)
            .When(config => config.Runtime != GameRuntime.Proton)
            .WithMessage("Proton version is only valid for Proton games.");

        RuleFor(config => config.Runtime)
            .Equal(GameRuntime.WindowsNative)
            .When(config => config.Launcher == GameLauncher.XboxPc)
            .WithMessage("Xbox PC games require a Windows runtime.");
    }
}
