using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Application.Features.CharacterVersions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.Wiki;

// Read-only, optimized for the frontend (project_overview.md §9, backend-planning.md §5.2).
public class WikiService(AppDbContext context, CharacterVersionService characterVersionService)
{
    public async Task<IEnumerable<WikiSagaDto>> GetSagasAsync(string? locale = null)
    {
        var sagas = await context.Sagas.OrderBy(s => s.Order).ToListAsync();

        return sagas.Select(s => new WikiSagaDto(
            s.Id, LocaleResolver.Resolve(s.Name, s.Translations, locale, t => t.Name), s.Order));
    }

    public async Task<IEnumerable<WikiArcDto>> GetArcsAsync(int? sagaId, string? locale = null)
    {
        var query = context.Arcs.AsQueryable();

        if (sagaId is not null)
        {
            query = query.Where(a => a.SagaId == sagaId);
        }

        // GlobalOrder, not Order: without a sagaId filter this list spans sagas, where
        // Order repeats per saga and would interleave arcs out of chronological order.
        var arcs = await query.OrderBy(a => a.GlobalOrder).ToListAsync();

        return arcs.Select(a => new WikiArcDto(
            a.Id, LocaleResolver.Resolve(a.Name, a.Translations, locale, t => t.Name), a.Order, a.GlobalOrder));
    }

    // RN11: the map loads every island ever revealed in one shot (no arcId filter), each
    // carrying FirstAppearanceGlobalOrder so the frontend can filter locally while scrubbing
    // a timeline without a network request per movement. Heavy content (events, resolved
    // character versions) stays gated behind GetIslandDetailsAsync's arcId requirement — RN08.
    public async Task<IEnumerable<WikiMapItemDto>> GetMapAsync(string? locale = null)
    {
        var islands = await context.Islands
            .Where(i => i.ArcIslands.Any())
            .Select(i => new
            {
                i.Id, i.Slug, i.Name, i.Translations, i.ThumbnailUrl, i.ModelUrl,
                i.CoordinateX, i.CoordinateY, i.CoordinateZ, i.RotationY, i.Scale,
                FirstAppearanceGlobalOrder = i.ArcIslands.Min(ai => ai.Arc.GlobalOrder)
            })
            .OrderBy(i => i.FirstAppearanceGlobalOrder)
            .ToListAsync();

        return islands.Select(i => new WikiMapItemDto(
            i.Id, i.Slug, LocaleResolver.Resolve(i.Name, i.Translations, locale, t => t.Name), i.ThumbnailUrl, i.ModelUrl,
            new CoordinatesDto(i.CoordinateX, i.CoordinateY, i.CoordinateZ),
            i.RotationY, i.Scale, i.FirstAppearanceGlobalOrder));
    }

    // RN08 (arcId required — enforced by the controllers via a non-nullable query param).
    // Used by GET /islands/{id}/details, where the caller already holds an id.
    public async Task<IslandDetailsDto> GetIslandDetailsAsync(int islandId, int arcId, string? locale = null)
    {
        var island = await context.Islands.FindAsync(islandId)
            ?? throw new NotFoundException($"Island {islandId} not found.");

        return await BuildIslandDetailsAsync(island, arcId, locale);
    }

    // Slug variant for GET /wiki/islands/{slug}/details: the public routes are
    // addressed by slug so URLs stay readable and stable, ids stay internal.
    public async Task<IslandDetailsDto> GetIslandDetailsBySlugAsync(string slug, int arcId, string? locale = null)
    {
        var island = await context.Islands.FirstOrDefaultAsync(i => i.Slug == slug)
            ?? throw new NotFoundException($"Island '{slug}' not found.");

        return await BuildIslandDetailsAsync(island, arcId, locale);
    }

    private async Task<IslandDetailsDto> BuildIslandDetailsAsync(Island island, int arcId, string? locale)
    {
        var islandId = island.Id;

        await EnsureArcExistsAsync(arcId);

        var islandName = LocaleResolver.Resolve(island.Name, island.Translations, locale, t => t.Name);
        var islandDescription = LocaleResolver.Resolve(island.Description, island.Translations, locale, t => t.Description);

        var arcIsland = await context.ArcIslands
            .FirstOrDefaultAsync(ai => ai.IslandId == islandId && ai.ArcId == arcId);

        if (arcIsland is null)
        {
            return new IslandDetailsDto(island.Id, islandName, islandDescription, [], []);
        }

        var events = await context.Events
            .Where(e => e.ArcIslandId == arcIsland.Id)
            .OrderBy(e => e.Order)
            .ToListAsync();

        var eventSummaries = events
            .Select(e => new EventSummaryDto(
                e.Id,
                LocaleResolver.Resolve(e.Title, e.Translations, locale, t => t.Title),
                LocaleResolver.Resolve(e.Description, e.Translations, locale, t => t.Description),
                e.Type.ToString(), e.Order))
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
            var effective = await characterVersionService.GetEffectiveVersionAsync(characterId, arcId, locale);
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

        return new IslandDetailsDto(island.Id, islandName, islandDescription, eventSummaries, characters);
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
