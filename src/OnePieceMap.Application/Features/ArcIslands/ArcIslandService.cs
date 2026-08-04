using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.ArcIslands;

public class ArcIslandService(AppDbContext context)
{
    public async Task<IEnumerable<ArcIslandDto>> GetByArcAsync(int arcId)
    {
        await EnsureArcExistsAsync(arcId);

        return await context.ArcIslands
            .Where(ai => ai.ArcId == arcId)
            .OrderBy(ai => ai.Order)
            .Select(ai => new ArcIslandDto(ai.Id, ai.ArcId, ai.IslandId, ai.Island.Name, ai.Order))
            .ToListAsync();
    }

    // RN06: an island can be linked to many arcs, one ArcIsland row per arc — no extra rule needed here.
    public async Task<ArcIslandDto> CreateAsync(int arcId, CreateArcIslandDto dto)
    {
        await EnsureArcExistsAsync(arcId);

        var island = await context.Islands.FindAsync(dto.IslandId)
            ?? throw new NotFoundException($"Island {dto.IslandId} not found.");

        var conflict = await context.ArcIslands
            .AnyAsync(ai => ai.ArcId == arcId && (ai.IslandId == dto.IslandId || ai.Order == dto.Order));
        if (conflict)
        {
            throw new ConflictException("This island is already linked to the arc, or the order is already taken.");
        }

        var arcIsland = new ArcIsland { ArcId = arcId, IslandId = dto.IslandId, Order = dto.Order };
        context.ArcIslands.Add(arcIsland);
        await context.SaveChangesAsync();

        return new ArcIslandDto(arcIsland.Id, arcIsland.ArcId, arcIsland.IslandId, island.Name, arcIsland.Order);
    }

    public async Task DeleteAsync(int arcId, int islandId)
    {
        var arcIsland = await context.ArcIslands
            .Include(ai => ai.Events)
            .FirstOrDefaultAsync(ai => ai.ArcId == arcId && ai.IslandId == islandId)
            ?? throw new NotFoundException($"No link between arc {arcId} and island {islandId}.");

        if (arcIsland.Events.Count > 0)
        {
            throw new ConflictException($"This arc-island link has {arcIsland.Events.Count} event(s) and cannot be removed.");
        }

        context.ArcIslands.Remove(arcIsland);
        await context.SaveChangesAsync();
    }

    private async Task EnsureArcExistsAsync(int arcId)
    {
        var exists = await context.Arcs.AnyAsync(a => a.Id == arcId);
        if (!exists)
        {
            throw new NotFoundException($"Arc {arcId} not found.");
        }
    }
}
