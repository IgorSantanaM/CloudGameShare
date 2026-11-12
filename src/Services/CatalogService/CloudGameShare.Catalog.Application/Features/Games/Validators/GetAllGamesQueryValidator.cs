using CloudGameShare.Catalog.Application.Features.Games.Queries.GetAllGames;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

public sealed class GetAllGamesQueryValidator : AbstractValidator<GetAllGamesQuery>
{
    public GetAllGamesQueryValidator()
    {
        RuleFor(query => query.PageNumber).GreaterThan(0).WithMessage("Page number must be at least 1.");
        RuleFor(query => query.PageSize).GreaterThan(0).WithMessage("Page size must be at least 1.");
    }
}
