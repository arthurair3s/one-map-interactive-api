namespace OnePieceMap.Domain.Entities;

// pivot: ilha revisitada por arco
public class ArcIsland
{
    public int Id { get; set; }
    public int ArcId { get; set; }
    public Arc Arc { get; set; } = null!;
    public int IslandId { get; set; }
    public Island Island { get; set; } = null!;
    public int Order { get; set; }               // ordem da rota naquele arco
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
