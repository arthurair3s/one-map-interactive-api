namespace OnePieceMap.Application.Features.Islands;

public record IslandDto(
    int Id, string Name, string Description,
    float CoordinateX, float CoordinateY, float CoordinateZ,
    float RotationY, float Scale, string ModelUrl, string ThumbnailUrl, bool IsActive);

public record CreateIslandDto(
    string Name, string Description,
    float CoordinateX, float CoordinateY, float CoordinateZ,
    float RotationY, float Scale, string ModelUrl, string ThumbnailUrl, bool IsActive);

public record UpdateIslandDto(
    string Name, string Description,
    float CoordinateX, float CoordinateY, float CoordinateZ,
    float RotationY, float Scale, string ModelUrl, string ThumbnailUrl, bool IsActive);
