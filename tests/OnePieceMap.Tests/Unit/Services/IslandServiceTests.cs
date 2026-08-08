using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Application.Features.Islands;
using OnePieceMap.Domain.Entities;

namespace OnePieceMap.Tests.Unit.Services;

// RN01: an Island can't be deleted while it's still linked to any arc via ArcIsland.
public class IslandServiceTests
{
    [Fact]
    public async Task DeleteAsync_IslandLinkedToArc_ThrowsConflict()
    {
        var context = TestDbContextFactory.Create();

        var saga = new Saga { Name = "East Blue", Order = 1 };
        context.Sagas.Add(saga);
        await context.SaveChangesAsync();

        var arc = new Arc { SagaId = saga.Id, Name = "Romance Dawn", Order = 1, GlobalOrder = 1 };
        context.Arcs.Add(arc);
        await context.SaveChangesAsync();

        var island = new Island
        {
            Name = "Fuscia Village", Slug = "fuscia-village", Description = "...", ModelUrl = "/m.glb", ThumbnailUrl = "/t.png", IsActive = true
        };
        context.Islands.Add(island);
        await context.SaveChangesAsync();

        context.ArcIslands.Add(new ArcIsland { ArcId = arc.Id, IslandId = island.Id, Order = 1 });
        await context.SaveChangesAsync();

        var service = new IslandService(context);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(island.Id));
    }

    [Fact]
    public async Task DeleteAsync_IslandNotLinkedToAnyArc_Succeeds()
    {
        var context = TestDbContextFactory.Create();

        var island = new Island
        {
            Name = "Fuscia Village", Slug = "fuscia-village", Description = "...", ModelUrl = "/m.glb", ThumbnailUrl = "/t.png", IsActive = true
        };
        context.Islands.Add(island);
        await context.SaveChangesAsync();

        var service = new IslandService(context);
        await service.DeleteAsync(island.Id);

        Assert.Empty(context.Islands);
    }
}
