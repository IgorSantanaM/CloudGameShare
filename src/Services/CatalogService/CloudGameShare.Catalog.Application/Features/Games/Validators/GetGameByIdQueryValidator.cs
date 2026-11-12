using CloudGameShare.Catalog.Application.Features.Games.Queries.GetGameById;
using FluentValidation;

namespace CloudGameShare.Catalog.Application.Features.Games.Validators;

public sealed class GetGameByIdQueryValidator : AbstractValidator<GetGameByIdQuery>
{
    public GetGameByIdQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty().WithMessage("Game ID is required.");
    }
}
