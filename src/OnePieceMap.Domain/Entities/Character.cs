namespace OnePieceMap.Domain.Entities;

public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;    // unique
    public ICollection<CharacterVersion> Versions { get; set; } = new List<CharacterVersion>();
}
