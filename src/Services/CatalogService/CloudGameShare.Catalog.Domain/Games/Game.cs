using CloudGameShare.BuildingBlocks.Domain.Exceptions;
using CloudGameShare.Catalog.Domain.Games.Events;
using CloudGameShare.Domain.Core.Models;

namespace CloudGameShare.Catalog.Domain.Games
{
    public sealed class Game : Entity<Guid>, IAggregateRoot
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public DateTime ReleaseDate { get; private set; }
        public string Genre { get; private set; } = string.Empty;
        public string Publisher { get; private set; } = string.Empty;
        public string Developer { get; private set; } = string.Empty;

        public string? CoverImageUrl { get; private set; }
        public string? TrailerUrl { get; private set; }
        public string? GameUrl { get; private set; }

        public bool IsPublished { get; private set; }

        public GameRequirements Requirements { get; private set; } = null!;
        public GameLaunchConfig LaunchConfig { get; private set; } = null!;

        // EF Core
        private Game() { }

        public Game(
            Guid id,
            string name,
            string? description,
            DateTime releaseDate,
            string genre,
            string publisher,
            string developer,
            GameRequirements requirements,
            GameLaunchConfig launchConfig,
            string? coverImageUrl = null,
            string? trailerUrl = null,
            string? gameUrl = null)
        {
            if (id == Guid.Empty)
                throw new DomainException("Invalid game ID.");

            Id = id;
            UpdateMetadata(name, description, releaseDate, genre, publisher, developer,
                coverImageUrl, trailerUrl, gameUrl);
            Requirements = requirements
                ?? throw new DomainException("Game requirements are required.");
            LaunchConfig = launchConfig
                ?? throw new DomainException("Game launch configuration is required.");
        }

        public void UpdateMetadata(
            string name,
            string? description,
            DateTime releaseDate,
            string genre,
            string publisher,
            string developer,
            string? coverImageUrl = null,
            string? trailerUrl = null,
            string? gameUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Game name is required.");

            if (string.IsNullOrWhiteSpace(genre))
                throw new DomainException("Game genre is required.");

            if (string.IsNullOrWhiteSpace(publisher))
                throw new DomainException("Game publisher is required.");

            if (string.IsNullOrWhiteSpace(developer))
                throw new DomainException("Game developer is required.");

            Name = name.Trim();
            Description = description;
            ReleaseDate = releaseDate;
            Genre = genre.Trim();
            Publisher = publisher.Trim();
            Developer = developer.Trim();

            CoverImageUrl = coverImageUrl;
            TrailerUrl = trailerUrl;
            GameUrl = gameUrl;
        }

        public void UpdateRequirements(GameRequirements requirements)
        {
            ArgumentNullException.ThrowIfNull(requirements);

            if (Requirements == requirements)
                return;

            Requirements = requirements;
        }

        public void UpdateLaunchConfig(GameLaunchConfig config)
        {
            ArgumentNullException.ThrowIfNull(config);

            if (LaunchConfig == config)
                return;

            LaunchConfig = config;
        }

        public void Publish()
        {
            if (IsPublished)
                return;

            IsPublished = true;

            AddDomainEvent(new GamePublishedEvent(Id));
        }

        public void Unpublish()
        {
            if (!IsPublished)
                return;

            IsPublished = false;
        }
    }
}
