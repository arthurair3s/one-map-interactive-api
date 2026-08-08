namespace OnePieceMap.Domain.Entities;

public class Island
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;    // unique, en (idioma padrão)
    public string Slug { get; set; } = null!;    // unique, estável e neutro de idioma — identifica a ilha na URL pública
    public string Description { get; set; } = null!; // en (idioma padrão)
    public Dictionary<string, IslandTranslation>? Translations { get; set; }
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
