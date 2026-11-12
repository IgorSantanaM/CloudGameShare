using CloudGameShare.Catalog.Application.Behaviors;
using CloudGameShare.Catalog.Application.Features.Games.Commands.CreateGame;
using CloudGameShare.Catalog.Application.Features.Games.Commands.DeleteGame;
using CloudGameShare.Catalog.Application.Features.Games.Commands.PublishGame;
using CloudGameShare.Catalog.Application.Features.Games.Commands.UnpublishGame;
using CloudGameShare.Catalog.Application.Features.Games.Commands.UpdateGame;
using CloudGameShare.Catalog.Application.Features.Games.Queries.GetAllGames;
using CloudGameShare.Catalog.Application.Features.Games.Queries.GetGameById;
using CloudGameShare.Catalog.Application.Features.Games.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CloudGameShare.Catalog.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCatalogApplication()
        {
            services.AddScoped<IValidator<CreateGameCommand>, CreateGameCommandValidator>();
            services.AddScoped<IValidator<UpdateGameCommand>, UpdateGameCommandValidator>();
            services.AddScoped<IValidator<DeleteGameCommand>, DeleteGameCommandValidator>();
            services.AddScoped<IValidator<PublishGameCommand>, PublishGameCommandValidator>();
            services.AddScoped<IValidator<UnpublishGameCommand>, UnpublishGameCommandValidator>();
            services.AddScoped<IValidator<GetGameByIdQuery>, GetGameByIdQueryValidator>();
            services.AddScoped<IValidator<GetAllGamesQuery>, GetAllGamesQueryValidator>();

            return services.AddMediatR(configuration =>
            {
                configuration.RegisterServicesFromAssemblyContaining(typeof(DependencyInjection));
                configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });
        }
    }
}
