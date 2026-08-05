using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Application.Features.Arcs;

public record ArcDto(int Id, int SagaId, string Name, int Order, int GlobalOrder);

public record CreateArcDto(int SagaId, string Name, int Order, int GlobalOrder, Dictionary<string, ArcTranslation>? Translations = null);

public record UpdateArcDto(int SagaId, string Name, int Order, int GlobalOrder, Dictionary<string, ArcTranslation>? Translations = null);
