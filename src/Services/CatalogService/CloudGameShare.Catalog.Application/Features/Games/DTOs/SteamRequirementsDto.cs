namespace CloudGameShare.Catalog.Application.Features.Games.DTOs;

public sealed record SteamRequirementsDto(
    SteamRequirementLevelDto? Minimum,
    SteamRequirementLevelDto? Recommended);

public sealed record SteamRequirementLevelDto(
    string Text,
    string? Processor,
    string? Graphics,
    int? CpuCores,
    int? MemoryGb,
    int? StorageGb);
