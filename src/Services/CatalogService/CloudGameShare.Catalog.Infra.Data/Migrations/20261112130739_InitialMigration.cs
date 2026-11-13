using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudGameShare.Catalog.Infra.Data.Migrations;

/// <inheritdoc />
public partial class _20261112130739_InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Games",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: true),
                ReleaseDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                Genre = table.Column<string>(type: "text", nullable: false),
                Publisher = table.Column<string>(type: "text", nullable: false),
                Developer = table.Column<string>(type: "text", nullable: false),
                CoverImageUrl = table.Column<string>(type: "text", nullable: true),
                TrailerUrl = table.Column<string>(type: "text", nullable: true),
                GameUrl = table.Column<string>(type: "text", nullable: true),
                IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                LaunchConfig_AppId = table.Column<string>(type: "text", nullable: false),
                LaunchConfig_Launcher = table.Column<int>(type: "integer", nullable: false),
                LaunchConfig_ProtonVersion = table.Column<string>(type: "text", nullable: true),
                LaunchConfig_Runtime = table.Column<int>(type: "integer", nullable: false),
                Requirements_CpuCores = table.Column<int>(type: "integer", nullable: false),
                Requirements_GpuCount = table.Column<int>(type: "integer", nullable: false),
                Requirements_MemoryGb = table.Column<int>(type: "integer", nullable: false),
                Requirements_StorageGb = table.Column<int>(type: "integer", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Games", x => x.Id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Games");
    }
}
