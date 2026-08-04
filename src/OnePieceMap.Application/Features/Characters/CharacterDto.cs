namespace OnePieceMap.Application.Features.Characters;

public record CharacterDto(int Id, string Name, string Slug);

public record CreateCharacterDto(string Name, string Slug);

public record UpdateCharacterDto(string Name, string Slug);
