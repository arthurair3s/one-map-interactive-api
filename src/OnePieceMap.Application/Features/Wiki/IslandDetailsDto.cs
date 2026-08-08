namespace OnePieceMap.Application.Features.Wiki;

public record IslandDetailsDto(
    int Id, string Name, string Description,
    IEnumerable<EventSummaryDto> Events,
    IEnumerable<CharacterAppearanceDto> Characters);

// Events now span every visit up to the active arc (RN09), so each one carries the arc it
// belongs to — a flat list mixing arcs would read as a single visit. Order is the event's
// position within its own visit; ArcGlobalOrder is what actually sequences the list.
public record EventSummaryDto(
    int Id, string Title, string Description, string Type, int Order,
    int ArcId, string ArcName, int ArcGlobalOrder);

public record CharacterAppearanceDto(
    int CharacterId, string Name, string Alias, string Epithet,
    long? Bounty, string Status, string Faction, string ImageUrl);
