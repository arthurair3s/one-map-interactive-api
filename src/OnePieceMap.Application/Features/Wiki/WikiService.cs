using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Application.Features.CharacterVersions;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.Wiki;

// Read-only, optimized for the frontend (project_overview.md §9, backend-planning.md §5.2).
public class WikiService(AppDbContext context, CharacterVersionService characterVersionService)
{
    public async Task<IEnumerable<WikiSagaDto>> GetSagasAsync()
    {
        return await context.Sagas
            .OrderBy(s => s.Order)
            .Select(s => new WikiSagaDto(s.Id, s.Name, s.Order))
            .ToListAsync();
    }

    public async Task<IEnumerable<WikiArcDto>> GetArcsAsync(int? sagaId)
    {
        var query = context.Arcs.AsQueryable();

        if (sagaId is not null)
        {
            query = query.Where(a => a.SagaId == sagaId);
        }

        return await query
            .OrderBy(a => a.Order)
            .Select(a => new WikiArcDto(a.Id, a.Name, a.Order))
            .ToListAsync();
    }

    // RN11: the map loads every island ever revealed in one shot (no arcId filter), each
    // carrying FirstAppearanceGlobalOrder so the frontend can filter locally while scrubbing
    // a timeline without a network request per movement. Heavy content (events, resolved
    // character versions) stays gated behind GetIslandDetailsAsync's arcId requirement — RN08.
    public async Task<IEnumerable<WikiMapItemDto>> GetMapAsync()
    {
        return await context.Islands
            .Where(i => i.ArcIslands.Any())
            .OrderBy(i => i.ArcIslands.Min(ai => ai.Arc.GlobalOrder))
            .Select(i => new WikiMapItemDto(
                i.Id, i.Name, i.ThumbnailUrl, i.ModelUrl,
                new CoordinatesDto(i.CoordinateX, i.CoordinateY, i.CoordinateZ),
                i.RotationY, i.Scale,
                i.ArcIslands.Min(ai => ai.Arc.GlobalOrder)))
            .ToListAsync();
    }

    // RN08 (arcId required — enforced by the controllers via a non-nullable query param).
    // Shared by GET /islands/{id}/details and GET /wiki/islands/{id}/details, which are
    // functionally identical (backend-planning.md §5.2).
    public async Task<IslandDetailsDto> GetIslandDetailsAsync(int islandId, int arcId)
    {
        var island = await context.Islands.FindAsync(islandId)
            ?? throw new NotFoundException($"Island {islandId} not found.");

        await EnsureArcExistsAsync(arcId);

        var arcIsland = await context.ArcIslands
            .FirstOrDefaultAsync(ai => ai.IslandId == islandId && ai.ArcId == arcId);

        if (arcIsland is null)
        {
            return new IslandDetailsDto(island.Id, island.Name, island.Description, [], []);
        }

        var events = await context.Events
            .Where(e => e.ArcIslandId == arcIsland.Id)
            .OrderBy(e => e.Order)
            .ToListAsync();

        var eventSummaries = events
            .Select(e => new EventSummaryDto(e.Id, e.Title, e.Description, e.Type.ToString(), e.Order))
            .ToList();

        var eventIds = events.Select(e => e.Id).ToList();

        var characterIds = await context.EventParticipants
            .Where(ep => eventIds.Contains(ep.EventId))
            .Select(ep => ep.CharacterVersion.CharacterId)
            .Distinct()
            .ToListAsync();

        var characters = new List<CharacterAppearanceDto>();
        foreach (var characterId in characterIds)
        {
            // RN05: show the character's version effective at this arc, not necessarily
            // the exact version recorded on the event's participation.
            var effective = await characterVersionService.GetEffectiveVersionAsync(characterId, arcId);
            if (effective is null)
            {
                continue;
            }

            var characterName = await context.Characters
                .Where(c => c.Id == characterId)
                .Select(c => c.Name)
                .FirstAsync();

            characters.Add(new CharacterAppearanceDto(
                characterId, characterName, effective.Alias, effective.Epithet,
                effective.Bounty, effective.Status, effective.Faction, effective.ImageUrl));
        }

        return new IslandDetailsDto(island.Id, island.Name, island.Description, eventSummaries, characters);
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
