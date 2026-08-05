using OnePieceMap.Domain.Enums;

namespace OnePieceMap.Domain.Entities;

public class CharacterVersion
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
    public int ArcId { get; set; }               // arco em que esta versão passa a valer
    public Arc Arc { get; set; } = null!;
    public string Alias { get; set; } = null!;
    public string Epithet { get; set; } = null!;
    public long? Bounty { get; set; }
    public CharacterStatus Status { get; set; }
    public Faction Faction { get; set; }
    public string ImageUrl { get; set; } = null!;
    public string Description { get; set; } = null!; // en (idioma padrão)
    public Dictionary<string, CharacterVersionTranslation>? Translations { get; set; }
    public ICollection<EventParticipant> Participations { get; set; } = new List<EventParticipant>();
}
