using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Domain.Enums;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.CharacterVersions;

public class CharacterVersionService(AppDbContext context)
{
    public async Task<IEnumerable<CharacterVersionDto>> GetByCharacterAsync(int characterId, string? locale = null)
    {
        await EnsureCharacterExistsAsync(characterId);

        var versions = await context.CharacterVersions
            .Include(cv => cv.Faction)
            .Where(cv => cv.CharacterId == characterId)
            .OrderBy(cv => cv.Arc.GlobalOrder)
            .ToListAsync();

        return versions.Select(v => ToDto(v, locale));
    }

    // RN04: unique CharacterVersion per (Character, Arc) pair.
    public async Task<CharacterVersionDto> CreateAsync(int characterId, CreateCharacterVersionDto dto)
    {
        await EnsureCharacterExistsAsync(characterId);
        await EnsureArcExistsAsync(dto.ArcId);
        await EnsureUniqueAsync(characterId, dto.ArcId, excludingId: null);
        var faction = await GetFactionAsync(dto.FactionId);

        var version = new CharacterVersion
        {
            CharacterId = characterId,
            ArcId = dto.ArcId,
            Alias = dto.Alias,
            Epithet = dto.Epithet,
            Bounty = dto.Bounty,
            Status = ParseStatus(dto.Status),
            Faction = faction,
            ImageUrl = dto.ImageUrl,
            Description = dto.Description,
            Translations = dto.Translations
        };
        context.CharacterVersions.Add(version);
        await context.SaveChangesAsync();

        return ToDto(version);
    }

    public async Task<CharacterVersionDto> UpdateAsync(int id, UpdateCharacterVersionDto dto)
    {
        var version = await context.CharacterVersions.FindAsync(id)
            ?? throw new NotFoundException($"CharacterVersion {id} not found.");

        await EnsureArcExistsAsync(dto.ArcId);
        await EnsureUniqueAsync(version.CharacterId, dto.ArcId, excludingId: id);
        var faction = await GetFactionAsync(dto.FactionId);

        version.ArcId = dto.ArcId;
        version.Alias = dto.Alias;
        version.Epithet = dto.Epithet;
        version.Bounty = dto.Bounty;
        version.Status = ParseStatus(dto.Status);
        version.Faction = faction;
        version.ImageUrl = dto.ImageUrl;
        version.Description = dto.Description;
        version.Translations = dto.Translations;
        await context.SaveChangesAsync();

        return ToDto(version);
    }

    // RN05: the version shown for (character, activeArc) is the one whose Arc.GlobalOrder
    // is the largest value <= the active arc's GlobalOrder. Null means not yet introduced.
    public async Task<CharacterVersionDto?> GetEffectiveVersionAsync(int characterId, int activeArcId, string? locale = null)
    {
        var activeArc = await context.Arcs.FindAsync(activeArcId)
            ?? throw new NotFoundException($"Arc {activeArcId} not found.");

        var version = await context.CharacterVersions
            .Include(cv => cv.Faction)
            .Where(cv => cv.CharacterId == characterId && cv.Arc.GlobalOrder <= activeArc.GlobalOrder)
            .OrderByDescending(cv => cv.Arc.GlobalOrder)
            .FirstOrDefaultAsync();

        return version is null ? null : ToDto(version, locale);
    }

    public async Task DeleteAsync(int id)
    {
        var version = await context.CharacterVersions
            .Include(cv => cv.Participations)
            .FirstOrDefaultAsync(cv => cv.Id == id)
            ?? throw new NotFoundException($"CharacterVersion {id} not found.");

        if (version.Participations.Count > 0)
        {
            throw new ConflictException($"CharacterVersion {id} has {version.Participations.Count} event participation(s) and cannot be deleted.");
        }

        context.CharacterVersions.Remove(version);
        await context.SaveChangesAsync();
    }

    private async Task EnsureCharacterExistsAsync(int characterId)
    {
        var exists = await context.Characters.AnyAsync(c => c.Id == characterId);
        if (!exists)
        {
            throw new NotFoundException($"Character {characterId} not found.");
        }
    }

    private async Task EnsureArcExistsAsync(int arcId)
    {
        var exists = await context.Arcs.AnyAsync(a => a.Id == arcId);
        if (!exists)
        {
            throw new NotFoundException($"Arc {arcId} not found.");
        }
    }

    private async Task EnsureUniqueAsync(int characterId, int arcId, int? excludingId)
    {
        var conflict = await context.CharacterVersions
            .Where(cv => excludingId == null || cv.Id != excludingId)
            .AnyAsync(cv => cv.CharacterId == characterId && cv.ArcId == arcId);

        if (conflict)
        {
            throw new ConflictException($"This character already has a version for arc {arcId}.");
        }
    }

    private async Task<Faction> GetFactionAsync(int factionId) =>
        await context.Factions.FindAsync(factionId)
            ?? throw new NotFoundException($"Faction {factionId} not found.");

    private static CharacterStatus ParseStatus(string status) => Enum.Parse<CharacterStatus>(status, ignoreCase: true);

    private static CharacterVersionDto ToDto(CharacterVersion cv, string? locale = null) => new(
        cv.Id, cv.CharacterId, cv.ArcId,
        LocaleResolver.Resolve(cv.Alias, cv.Translations, locale, t => t.Alias),
        LocaleResolver.Resolve(cv.Epithet, cv.Translations, locale, t => t.Epithet),
        cv.Bounty, cv.Status.ToString(),
        LocaleResolver.Resolve(cv.Faction.Name, cv.Faction.Translations, locale, t => t.Name),
        cv.ImageUrl,
        LocaleResolver.Resolve(cv.Description, cv.Translations, locale, t => t.Description));
}
