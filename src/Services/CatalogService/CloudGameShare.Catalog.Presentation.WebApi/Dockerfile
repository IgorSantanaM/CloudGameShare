FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 8082

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["src/Services/CatalogService/CloudGameShare.Catalog.Presentation.WebApi/CloudGameShare.Catalog.Presentation.WebApi.csproj", "src/Services/CatalogService/CloudGameShare.Catalog.Presentation.WebApi/"]
COPY ["src/Contracts/CloudGameShare.Catalog.Contracts/CloudGameShare.Catalog.Contracts.csproj", "src/Contracts/CloudGameShare.Catalog.Contracts/"]
COPY ["src/Services/GamingSessionService/CloudGameShare.GamingSession.Infra.Services/CloudGameShare.GamingSession.Infra.Services.csproj", "src/Services/GamingSessionService/CloudGameShare.GamingSession.Infra.Services/"]
COPY ["src/Services/GamingSessionService/CloudGameShare.GamingSession.Application/CloudGameShare.GamingSession.Application.csproj", "src/Services/GamingSessionService/CloudGameShare.GamingSession.Application/"]
COPY ["src/Services/GamingSessionService/CloudGameShare.GamingSession.Domain/CloudGameShare.GamingSession.Domain.csproj", "src/Services/GamingSessionService/CloudGameShare.GamingSession.Domain/"]
COPY ["src/Services/CatalogService/CloudGameShare.Catalog.Application/CloudGameShare.Catalog.Application.csproj", "src/Services/CatalogService/CloudGameShare.Catalog.Application/"]
COPY ["src/Services/CatalogService/CloudGameShare.Catalog.Domain/CloudGameShare.Catalog.Domain.csproj", "src/Services/CatalogService/CloudGameShare.Catalog.Domain/"]
COPY ["src/BuildingBlocks/CloudGameShare.BuildingBlocks.Domain/CloudGameShare.BuildingBlocks.Domain.csproj", "src/BuildingBlocks/CloudGameShare.BuildingBlocks.Domain/"]
COPY ["src/Services/CatalogService/CloudGameShare.Catalog.Infra.Data/CloudGameShare.Catalog.Infra.Data.csproj", "src/Services/CatalogService/CloudGameShare.Catalog.Infra.Data/"]
COPY ["src/Services/CatalogService/CloudGameShare.Catalog.Infra.Services/CloudGameShare.Catalog.Infra.Services.csproj", "src/Services/CatalogService/CloudGameShare.Catalog.Infra.Services/"]
COPY ["src/BuildingBlocks/CloudGameShare.BuildingBlocks.Infra/CloudGameShare.BuildingBlocks.Infra.csproj", "src/BuildingBlocks/CloudGameShare.BuildingBlocks.Infra/"]

RUN --mount=type=cache,id=cloudgameshare-nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "./src/Services/CatalogService/CloudGameShare.Catalog.Presentation.WebApi/CloudGameShare.Catalog.Presentation.WebApi.csproj"
COPY . .

WORKDIR "/src/src/Services/CatalogService/CloudGameShare.Catalog.Presentation.WebApi"

RUN --mount=type=cache,id=cloudgameshare-nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet build "./CloudGameShare.Catalog.Presentation.WebApi.csproj" --no-restore -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN --mount=type=cache,id=cloudgameshare-nuget,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "./CloudGameShare.Catalog.Presentation.WebApi.csproj" --no-restore -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish  --chown=app:app /app/publish .

USER app

ENTRYPOINT ["dotnet", "CloudGameShare.Catalog.Presentation.WebApi.dll"]
