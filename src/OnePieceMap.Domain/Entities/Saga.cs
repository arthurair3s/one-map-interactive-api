namespace OnePieceMap.Domain.Entities;

public class Saga
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;   // unique
    public int Order { get; set; }              // unique
    public ICollection<Arc> Arcs { get; set; } = new List<Arc>();
}
