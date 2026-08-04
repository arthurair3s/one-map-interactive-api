namespace OnePieceMap.Application.Features.Wiki;

public record WikiMapItemDto(
    int Id, string Name, string ThumbnailUrl, string ModelUrl,
    CoordinatesDto Coordinates, float RotationY, float Scale,
    int FirstAppearanceGlobalOrder);

public record CoordinatesDto(float X, float Y, float Z);
