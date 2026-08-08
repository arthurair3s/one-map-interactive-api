using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.Factions;

public class FactionService(AppDbContext context)
{
    public async Task<PagedResult<FactionDto>> GetAllAsync(int page, int pageSize, string? locale = null)
    {
        var query = context.Factions.OrderBy(f => f.Name);

        var total = await query.CountAsync();
        var factions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = factions.Select(f => ToDto(f, locale));

        return new PagedResult<FactionDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<FactionDto> GetByIdAsync(int id, string? locale = null)
    {
        var faction = await context.Factions.FindAsync(id)
            ?? throw new NotFoundException($"Faction {id} not found.");

        return ToDto(faction, locale);
    }

    public async Task<FactionDto> CreateAsync(CreateFactionDto dto)
    {
        await EnsureUniqueAsync(dto.Name, dto.Slug, excludingId: null);

        var faction = new Faction { Name = dto.Name, Slug = dto.Slug, Translations = dto.Translations };
        context.Factions.Add(faction);
        await context.SaveChangesAsync();

        return ToDto(faction);
    }

    public async Task<FactionDto> UpdateAsync(int id, UpdateFactionDto dto)
    {
        var faction = await context.Factions.FindAsync(id)
            ?? throw new NotFoundException($"Faction {id} not found.");

        await EnsureUniqueAsync(dto.Name, dto.Slug, excludingId: id);

        faction.Name = dto.Name;
        faction.Slug = dto.Slug;
        faction.Translations = dto.Translations;
        await context.SaveChangesAsync();

        return ToDto(faction);
    }

    // RN01-style guard: a faction still referenced by a CharacterVersion can't be removed
    // out from under it — same pattern as Character/Arc/Saga deletion.
    public async Task DeleteAsync(int id)
    {
        var faction = await context.Factions
            .Include(f => f.CharacterVersions)
            .FirstOrDefaultAsync(f => f.Id == id)
            ?? throw new NotFoundException($"Faction {id} not found.");

        if (faction.CharacterVersions.Count > 0)
        {
            throw new ConflictException($"Faction {id} has {faction.CharacterVersions.Count} character version(s) and cannot be deleted.");
        }

        context.Factions.Remove(faction);
        await context.SaveChangesAsync();
    }

    private async Task EnsureUniqueAsync(string name, string slug, int? excludingId)
    {
        var conflict = await context.Factions
            .Where(f => excludingId == null || f.Id != excludingId)
            .Where(f => f.Name == name || f.Slug == slug)
            .FirstOrDefaultAsync();

        if (conflict is not null)
        {
            var field = conflict.Name == name ? "Name" : "Slug";
            throw new ConflictException($"Another faction already uses this {field}.");
        }
    }

    private static FactionDto ToDto(Faction f, string? locale = null) => new(
        f.Id, LocaleResolver.Resolve(f.Name, f.Translations, locale, t => t.Name), f.Slug);
}
