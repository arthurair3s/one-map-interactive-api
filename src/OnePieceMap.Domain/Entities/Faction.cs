namespace OnePieceMap.Domain.Entities;

public class Faction
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;    // unique, en (idioma padrão)
    public string Slug { get; set; } = null!;    // unique, estável e neutro de idioma
    public Dictionary<string, FactionTranslation>? Translations { get; set; }
    public ICollection<CharacterVersion> CharacterVersions { get; set; } = new List<CharacterVersion>();
}
