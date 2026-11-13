using CloudGameShare.BuildingBlocks.Infra.Extensions;
using CloudGameShare.Catalog.Domain.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CloudGameShare.Catalog.Infra.Data.Mappings;

public sealed class GameMapping : EntityTypeConfigurationExtension<Game>
{
    public override void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(game => game.Id);
        builder.Property(game => game.Id).ValueGeneratedNever();
        builder.Ignore(game => game.DomainEvents);

        builder.Property(game => game.Name).IsRequired();
        builder.Property(game => game.Genre).IsRequired();
        builder.Property(game => game.Publisher).IsRequired();
        builder.Property(game => game.Developer).IsRequired();
        builder.Property(game => game.ReleaseDate)
            .HasColumnType("timestamp without time zone")
            .HasConversion(date => DateTime.SpecifyKind(date, DateTimeKind.Unspecified), date => date);

        builder.ComplexProperty(game => game.Requirements, requirements =>
        {
            requirements.IsRequired();
            requirements.Property(value => value.CpuCores);
            requirements.Property(value => value.MemoryGb);
            requirements.Property(value => value.GpuCount);
            requirements.Property(value => value.StorageGb);
        });

        builder.ComplexProperty(game => game.LaunchConfig, launch =>
        {
            launch.IsRequired();
            launch.Property(value => value.Launcher);
            launch.Property(value => value.AppId).IsRequired();
            launch.Property(value => value.Runtime);
            launch.Property(value => value.ProtonVersion);
        });
    }
}
