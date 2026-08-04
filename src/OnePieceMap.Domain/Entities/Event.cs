using OnePieceMap.Domain.Enums;

namespace OnePieceMap.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public int ArcIslandId { get; set; }
    public ArcIsland ArcIsland { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public EventType Type { get; set; }          // Lore | Combat
    public int Order { get; set; }               // unique por ArcIslandId
    public ICollection<EventParticipant> Participants { get; set; } = new List<EventParticipant>();
}
