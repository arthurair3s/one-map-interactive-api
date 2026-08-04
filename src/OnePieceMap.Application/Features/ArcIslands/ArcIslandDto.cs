namespace OnePieceMap.Application.Features.ArcIslands;

public record ArcIslandDto(int Id, int ArcId, int IslandId, string IslandName, int Order);

public record CreateArcIslandDto(int IslandId, int Order);
