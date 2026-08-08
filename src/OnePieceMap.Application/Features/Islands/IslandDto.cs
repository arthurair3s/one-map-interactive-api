using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Application.Features.Islands;

public record IslandDto(
    int Id, string Name, string Slug, string Description,
    float CoordinateX, float CoordinateY, float CoordinateZ,
    float RotationY, float Scale, string ModelUrl, string ThumbnailUrl, bool IsActive);

public record CreateIslandDto(
    string Name, string Slug, string Description,
    float CoordinateX, float CoordinateY, float CoordinateZ,
    float RotationY, float Scale, string ModelUrl, string ThumbnailUrl, bool IsActive,
    Dictionary<string, IslandTranslation>? Translations = null);

public record UpdateIslandDto(
    string Name, string Slug, string Description,
    float CoordinateX, float CoordinateY, float CoordinateZ,
    float RotationY, float Scale, string ModelUrl, string ThumbnailUrl, bool IsActive,
    Dictionary<string, IslandTranslation>? Translations = null);
