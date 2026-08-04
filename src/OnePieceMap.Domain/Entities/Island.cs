namespace OnePieceMap.Domain.Entities;

public class Island
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;    // unique
    public string Description { get; set; } = null!;
    public float CoordinateX { get; set; }
    public float CoordinateY { get; set; }
    public float CoordinateZ { get; set; }
    public float RotationY { get; set; }         // graus
    public float Scale { get; set; }
    public string ModelUrl { get; set; } = null!;
    public string ThumbnailUrl { get; set; } = null!;
    public bool IsActive { get; set; }
    public ICollection<ArcIsland> ArcIslands { get; set; } = new List<ArcIsland>();
}
