using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Application.Features.Factions;

public record FactionDto(int Id, string Name, string Slug);

public record CreateFactionDto(string Name, string Slug, Dictionary<string, FactionTranslation>? Translations = null);

public record UpdateFactionDto(string Name, string Slug, Dictionary<string, FactionTranslation>? Translations = null);
