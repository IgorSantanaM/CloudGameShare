using CloudGameShare.Catalog.Domain.Games;

namespace CloudGameShare.Catalog.Application.Features.Games.DTOs;

public sealed record GameRequirementsDto(int CpuCores, int MemoryGb, int GpuCount, int StorageGb)
{
    internal GameRequirements ToDomain() => new(CpuCores, MemoryGb, GpuCount, StorageGb);

    internal static GameRequirementsDto From(GameRequirements requirements)
        => new(requirements.CpuCores, requirements.MemoryGb, requirements.GpuCount, requirements.StorageGb);
}
