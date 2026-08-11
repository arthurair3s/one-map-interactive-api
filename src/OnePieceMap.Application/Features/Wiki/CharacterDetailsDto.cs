namespace OnePieceMap.Application.Features.Wiki;

public record CharacterDetailsDto(int Id, string Name, string Slug, IEnumerable<CharacterVersionDetailsDto> Versions);

// Every version reached so far (RN09-style, not RN05's single "effective" version) — the
// carousel lets the reader browse how the character evolved up to the active arc, so each
// entry carries the arc it belongs to, same reasoning as EventSummaryDto.
public record CharacterVersionDetailsDto(
    int Id, string Alias, string Epithet, long? Bounty, string Status, string Faction, string ImageUrl, string Description,
    int ArcId, string ArcName, int ArcGlobalOrder);
