namespace OnePieceMap.Application.Features.CharacterVersions;

public record CharacterVersionDto(
    int Id, int CharacterId, int ArcId,
    string Alias, string Epithet, long? Bounty, string Status, string Faction,
    string ImageUrl, string Description);

public record CreateCharacterVersionDto(
    int ArcId, string Alias, string Epithet, long? Bounty, string Status, string Faction,
    string ImageUrl, string Description);

public record UpdateCharacterVersionDto(
    int ArcId, string Alias, string Epithet, long? Bounty, string Status, string Faction,
    string ImageUrl, string Description);
