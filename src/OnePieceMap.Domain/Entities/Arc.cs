namespace OnePieceMap.Domain.Entities;

public class Arc
{
    public int Id { get; set; }
    public int SagaId { get; set; }
    public Saga Saga { get; set; } = null!;
    public string Name { get; set; } = null!;     // en (idioma padrão)
    public int Order { get; set; }               // unique por SagaId — numeração do arco DENTRO da saga (UI)
    public int GlobalOrder { get; set; }          // unique global — posição absoluta na história completa (regras de negócio)
    public Dictionary<string, ArcTranslation>? Translations { get; set; }
    public ICollection<ArcIsland> ArcIslands { get; set; } = new List<ArcIsland>();
    public ICollection<CharacterVersion> CharacterVersions { get; set; } = new List<CharacterVersion>();
}
