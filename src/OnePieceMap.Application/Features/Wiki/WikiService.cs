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

        var activeArc = await context.Arcs.FirstOrDefaultAsync(a => a.Id == arcId)
            ?? throw new NotFoundException($"Arc {arcId} not found.");

        var islandName = LocaleResolver.Resolve(island.Name, island.Translations, locale, t => t.Name);
        var islandDescription = LocaleResolver.Resolve(island.Description, island.Translations, locale, t => t.Description);

        // RN09: every visit the journey has already reached, not just the arc that happens to
        // be active. An island passed at arc 3 keeps showing what happened there once the reader
        // is at arc 6 — only later visits stay hidden. GlobalOrder, never Order (§8).
        var visits = await context.ArcIslands
            .Include(ai => ai.Arc)
            .Where(ai => ai.IslandId == islandId && ai.Arc.GlobalOrder <= activeArc.GlobalOrder)
            .ToListAsync();

        if (visits.Count == 0)
        {
            return new IslandDetailsDto(island.Id, islandName, islandDescription, [], []);
        }

        var visitById = visits.ToDictionary(v => v.Id);
        var visitIds = visitById.Keys.ToList();

        var events = await context.Events
            .Where(e => visitIds.Contains(e.ArcIslandId))
            .ToListAsync();

        // Chronological across visits: the arc's absolute position first, then the island's
        // place in that arc's route, then the event's own order within the visit.
        var eventSummaries = events
            .OrderBy(e => visitById[e.ArcIslandId].Arc.GlobalOrder)
            .ThenBy(e => visitById[e.ArcIslandId].Order)
            .ThenBy(e => e.Order)
            .Select(e =>
            {
                var arc = visitById[e.ArcIslandId].Arc;

                return new EventSummaryDto(
                    e.Id,
                    LocaleResolver.Resolve(e.Title, e.Translations, locale, t => t.Title),
                    LocaleResolver.Resolve(e.Description, e.Translations, locale, t => t.Description),
                    e.Type.ToString(), e.Order,
                    arc.Id, LocaleResolver.Resolve(arc.Name, arc.Translations, locale, t => t.Name), arc.GlobalOrder);
            })
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

            var characterInfo = await context.Characters
                .Where(c => c.Id == characterId)
                .Select(c => new { c.Name, c.Slug })
                .FirstAsync();

            characters.Add(new CharacterAppearanceDto(
                characterId, characterInfo.Slug, characterInfo.Name, effective.Alias, effective.Epithet,
                effective.Bounty, effective.Status, effective.Faction, effective.ImageUrl));
        }

        return new IslandDetailsDto(island.Id, islandName, islandDescription, eventSummaries, characters);
    }

    // Slug variant for GET /wiki/characters/{slug}/details: same public/readable-URL
    // reasoning as GetIslandDetailsBySlugAsync.
    public async Task<CharacterDetailsDto> GetCharacterDetailsBySlugAsync(string slug, int arcId, string? locale = null)
    {
        var character = await context.Characters.FirstOrDefaultAsync(c => c.Slug == slug)
            ?? throw new NotFoundException($"Character '{slug}' not found.");

        var activeArc = await context.Arcs.FirstOrDefaultAsync(a => a.Id == arcId)
            ?? throw new NotFoundException($"Arc {arcId} not found.");

        // RN09-style accumulation applied to CharacterVersion instead of Event: every version
        // whose Arc.GlobalOrder the reader has already reached, not just the one currently
        // effective (that's RN05, used elsewhere for a single resolved version) — the carousel
        // shows the character's revealed history so far.
        var versions = await context.CharacterVersions
            .Include(cv => cv.Arc)
            .Include(cv => cv.Faction)
            .Where(cv => cv.CharacterId == character.Id && cv.Arc.GlobalOrder <= activeArc.GlobalOrder)
            .OrderBy(cv => cv.Arc.GlobalOrder)
            .ToListAsync();

        var versionDtos = versions.Select(v => new CharacterVersionDetailsDto(
            v.Id,
            LocaleResolver.Resolve(v.Alias, v.Translations, locale, t => t.Alias),
            LocaleResolver.Resolve(v.Epithet, v.Translations, locale, t => t.Epithet),
            v.Bounty, v.Status.ToString(),
            LocaleResolver.Resolve(v.Faction.Name, v.Faction.Translations, locale, t => t.Name),
            v.ImageUrl,
            LocaleResolver.Resolve(v.Description, v.Translations, locale, t => t.Description),
            v.Arc.Id, LocaleResolver.Resolve(v.Arc.Name, v.Arc.Translations, locale, t => t.Name), v.Arc.GlobalOrder));

        return new CharacterDetailsDto(character.Id, character.Name, character.Slug, versionDtos);
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
