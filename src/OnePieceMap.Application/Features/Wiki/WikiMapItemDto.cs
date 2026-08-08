namespace OnePieceMap.Application.Features.Wiki;

// Slug travels with the map payload so clicking an island can build its public URL
// without a second round-trip just to resolve id → slug.
public record WikiMapItemDto(
    int Id, string Slug, string Name, string ThumbnailUrl, string ModelUrl,
    CoordinatesDto Coordinates, float RotationY, float Scale,
    int FirstAppearanceGlobalOrder);

public record CoordinatesDto(float X, float Y, float Z);
