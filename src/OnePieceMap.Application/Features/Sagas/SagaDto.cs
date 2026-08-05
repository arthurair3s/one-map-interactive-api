using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Application.Features.Sagas;

public record SagaDto(int Id, string Name, int Order);

public record CreateSagaDto(string Name, int Order, Dictionary<string, SagaTranslation>? Translations = null);

public record UpdateSagaDto(string Name, int Order, Dictionary<string, SagaTranslation>? Translations = null);
