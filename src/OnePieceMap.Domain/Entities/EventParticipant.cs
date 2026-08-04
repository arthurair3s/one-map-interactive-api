namespace OnePieceMap.Domain.Entities;

// pivot: PK composta (EventId, CharacterVersionId)
public class EventParticipant
{
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int CharacterVersionId { get; set; }
    public CharacterVersion CharacterVersion { get; set; } = null!;
}
