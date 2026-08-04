namespace OnePieceMap.Application.Features.Wiki;

public record IslandDetailsDto(
    int Id, string Name, string Description,
    IEnumerable<EventSummaryDto> Events,
    IEnumerable<CharacterAppearanceDto> Characters);

public record EventSummaryDto(int Id, string Title, string Description, string Type, int Order);

public record CharacterAppearanceDto(
    int CharacterId, string Name, string Alias, string Epithet,
    long? Bounty, string Status, string Faction, string ImageUrl);
