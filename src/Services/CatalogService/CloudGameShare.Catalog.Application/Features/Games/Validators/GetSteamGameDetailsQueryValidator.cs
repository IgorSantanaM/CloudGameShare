using CloudGameShare.Catalog.Application.Features.Games.Queries.GetSteamGameDetails;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

public sealed class GetSteamGameDetailsQueryValidator : AbstractValidator<GetSteamGameDetailsQuery>
{
    public GetSteamGameDetailsQueryValidator()
    {
        RuleFor(query => query.AppId).GreaterThan(0u).WithMessage("Steam App ID must be a positive numeric identifier.");

        RuleFor(query => query.Language).Must(language => language is "english" or "portuguese" or "brazilian")
            .WithMessage("Choose English, Portuguese, or Brazilian Portuguese.");
    }
}
