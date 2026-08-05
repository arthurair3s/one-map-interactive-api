namespace OnePieceMap.Domain.Entities;

public class Saga
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;   // unique, en (idioma padrão)
    public int Order { get; set; }              // unique
    public Dictionary<string, SagaTranslation>? Translations { get; set; }
    public ICollection<Arc> Arcs { get; set; } = new List<Arc>();
}
