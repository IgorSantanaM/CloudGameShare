using CloudGameShare.GamingSession.Application.Features.Sessions.Queries.GetSessionStatus;
using FluentValidation;

namespace CloudGameShare.GamingSession.Application.Features.Sessions.Validators;

public sealed class GetSessionStatusQueryValidator : AbstractValidator<GetSessionStatusQuery>
{
    public GetSessionStatusQueryValidator()
    {
        RuleFor(query => query.OwnerId).NotEmpty().WithMessage("An authenticated player is required.");
        RuleFor(query => query.Id).NotEmpty().WithMessage("Session ID is required.");
    }
}
