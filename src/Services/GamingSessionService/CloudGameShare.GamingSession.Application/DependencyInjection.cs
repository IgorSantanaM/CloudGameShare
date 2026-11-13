using CloudGameShare.GamingSession.Application.Features.Sessions.Commands.CreateSession;
using CloudGameShare.GamingSession.Application.Features.Sessions.Queries.GetSessionStatus;
using CloudGameShare.GamingSession.Application.Features.Sessions.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CloudGameShare.GamingSession.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGamingSessionApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IValidator<CreateSessionCommand>, CreateSessionCommandValidator>();
        services.AddScoped<IValidator<GetSessionStatusQuery>, GetSessionStatusQueryValidator>();

        return services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblyContaining<CreateSessionCommand>();
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
    }
}

internal sealed class ValidationBehavior<TRequest, TResponse>(IValidator<TRequest> validator)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        return await next(cancellationToken);
    }
}
