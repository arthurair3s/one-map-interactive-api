namespace OnePieceMap.Application.Features.Sagas;

public record SagaDto(int Id, string Name, int Order);

public record CreateSagaDto(string Name, int Order);

public record UpdateSagaDto(string Name, int Order);
