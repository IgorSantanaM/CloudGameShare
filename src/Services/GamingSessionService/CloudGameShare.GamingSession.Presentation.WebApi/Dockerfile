FROM mcr.microsoft.com/dotnet/aspnet:11.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:11.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/GamingSessionService/CloudGameShare.GamingSession.Presentation.WebApi/CloudGameShare.GamingSession.Presentation.WebApi.csproj", "src/Services/GamingSessionService/CloudGameShare.GamingSession.Presentation.WebApi/"]
COPY ["src/Services/GamingSessionService/CloudGameShare.GamingSession.Application/CloudGameShare.GamingSession.Application.csproj", "src/Services/GamingSessionService/CloudGameShare.GamingSession.Application/"]
COPY ["src/Services/GamingSessionService/CloudGameShare.GamingSession.Domain/CloudGameShare.GamingSession.Domain.csproj", "src/Services/GamingSessionService/CloudGameShare.GamingSession.Domain/"]
COPY ["src/Services/GamingSessionService/CloudGameShare.GamingSession.Infra.Data/CloudGameShare.GamingSession.Infra.Data.csproj", "src/Services/GamingSessionService/CloudGameShare.GamingSession.Infra.Data/"]
COPY ["src/Services/GamingSessionService/CloudGameShare.GamingSession.Infra.Services/CloudGameShare.GamingSession.Infra.Services.csproj", "src/Services/GamingSessionService/CloudGameShare.GamingSession.Infra.Services/"]
COPY ["src/Contracts/CloudGameShare.Catalog.Contracts/CloudGameShare.Catalog.Contracts.csproj", "src/Contracts/CloudGameShare.Catalog.Contracts/"]
RUN dotnet restore "./src/Services/GamingSessionService/CloudGameShare.GamingSession.Presentation.WebApi/CloudGameShare.GamingSession.Presentation.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Services/GamingSessionService/CloudGameShare.GamingSession.Presentation.WebApi"
RUN dotnet build "./CloudGameShare.GamingSession.Presentation.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CloudGameShare.GamingSession.Presentation.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish --chown=app:app /app/publish .
USER app
ENTRYPOINT ["dotnet", "CloudGameShare.GamingSession.Presentation.WebApi.dll"]