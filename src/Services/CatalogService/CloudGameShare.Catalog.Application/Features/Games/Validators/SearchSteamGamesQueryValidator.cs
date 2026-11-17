using CloudGameShare.Catalog.Application.Features.Games.Queries.SearchSteamGames;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

public sealed class SearchSteamGamesQueryValidator : AbstractValidator<SearchSteamGamesQuery>
{
    public SearchSteamGamesQueryValidator()
    {
        RuleFor(query => query.Name)
            .NotEmpty().WithMessage("Enter a game name.")
            .Must(name => name?.Trim().Length >= 2).WithMessage("Enter at least two characters.")
            .MaximumLength(100).WithMessage("Game search cannot exceed 100 characters.");

        RuleFor(query => query.Language).Must(language => language is "english" or "portuguese" or "brazilian")
            .WithMessage("Choose English, Portuguese, or Brazilian Portuguese.");
    }
}
