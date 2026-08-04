namespace OnePieceMap.Application.Features.Events;

public record EventDto(int Id, int ArcIslandId, string Title, string Description, string Type, int Order);

public record EventDetailDto(
    int Id, int ArcIslandId, string Title, string Description, string Type, int Order,
    IEnumerable<int> ParticipantCharacterVersionIds);

public record CreateEventDto(int ArcIslandId, string Title, string Description, string Type, int Order);

public record UpdateEventDto(int ArcIslandId, string Title, string Description, string Type, int Order);
