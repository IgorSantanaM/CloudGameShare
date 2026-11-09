using CloudGameShare.BuildingBlocks.Domain.Exceptions;
using CloudGameShare.Catalog.Domain.NewFolder;
using CloudGameShare.Domain.Core.Models;

namespace CloudGameShare.Catalog.Domain.Games
{
    public class Game : Entity<Guid>, IAggregateRoot    
    {
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; } = string.Empty;
        public string AppId { get; private set; } = string.Empty;
        /// <summary>
        /// Stores the platforms associated with the containing object.
        /// One game can be available on multiple platforms.
        /// </summary>
        private readonly List<Platform> _platforms = new List<Platform>();
        public IReadOnlyCollection<Platform> Platforms => _platforms.AsReadOnly();
        public DateTime ReleaseDate { get; private set; }
        public string Genre { get; private set; } = string.Empty;
        public string Publisher { get; private set; } = string.Empty;
        public string Developer { get; private set; } = string.Empty;
        public string? CoverImageUrl { get; private set; } = string.Empty;
        public string? TrailerUrl { get; private set; } = string.Empty;
        public string? GameUrl { get; private set; } = string.Empty;
        public DateTime LastPlayed { get; private set; }

        public Game(string name, string description, string appId, DateTime releaseDate, string genre, string publisher, string developer, string coverImageUrl, string trailerUrl, string gameUrl)
        {
            if(string.IsNullOrWhiteSpace(name))
                throw new DomainException("Game name cannot be null or empty.");
            Name = name;
            Description = description;
            AppId = appId;
            ReleaseDate = releaseDate;
            Genre = genre;
            Publisher = publisher;
            Developer = developer;
            CoverImageUrl = coverImageUrl;
            TrailerUrl = trailerUrl;
            GameUrl = gameUrl;
            LastPlayed = DateTime.MinValue; 
        }
    }
}
