namespace OnePieceMap.Domain.Entities;

public class Arc
{
    public int Id { get; set; }
    public int SagaId { get; set; }
    public Saga Saga { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Order { get; set; }               // unique por SagaId — numeração do arco DENTRO da saga (UI)
    public int GlobalOrder { get; set; }          // unique global — posição absoluta na história completa (regras de negócio)
    public ICollection<ArcIsland> ArcIslands { get; set; } = new List<ArcIsland>();
    public ICollection<CharacterVersion> CharacterVersions { get; set; } = new List<CharacterVersion>();
}
