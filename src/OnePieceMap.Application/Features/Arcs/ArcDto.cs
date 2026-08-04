namespace OnePieceMap.Application.Features.Arcs;

public record ArcDto(int Id, int SagaId, string Name, int Order, int GlobalOrder);

public record CreateArcDto(int SagaId, string Name, int Order, int GlobalOrder);

public record UpdateArcDto(int SagaId, string Name, int Order, int GlobalOrder);
