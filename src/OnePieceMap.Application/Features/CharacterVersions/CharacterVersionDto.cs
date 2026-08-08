using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Application.Features.CharacterVersions;

// Faction stays a resolved, translated display string on read — same treatment as
// Alias/Epithet/Description — even though it's now backed by a table (RN: see Faction
// entity). Create/Update take FactionId instead, consistent with how ArcId is already a
// plain FK reference rather than a resolved name.
public record CharacterVersionDto(
    int Id, int CharacterId, int ArcId,
    string Alias, string Epithet, long? Bounty, string Status, string Faction,
    string ImageUrl, string Description);

public record CreateCharacterVersionDto(
    int ArcId, string Alias, string Epithet, long? Bounty, string Status, int FactionId,
    string ImageUrl, string Description, Dictionary<string, CharacterVersionTranslation>? Translations = null);

public record UpdateCharacterVersionDto(
    int ArcId, string Alias, string Epithet, long? Bounty, string Status, int FactionId,
    string ImageUrl, string Description, Dictionary<string, CharacterVersionTranslation>? Translations = null);
