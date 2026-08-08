namespace OnePieceMap.Application.Features.Wiki;

public record WikiSagaDto(int Id, string Name, int Order);

// GlobalOrder is what the frontend compares against WikiMapItemDto.FirstAppearanceGlobalOrder
// to filter islands while scrubbing the timeline (RN11). Order is saga-local and must not be
// used for chronology — see backend-planning.md §8.
public record WikiArcDto(int Id, string Name, int Order, int GlobalOrder);
