using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.Islands;

public class IslandService(AppDbContext context)
{
    public async Task<PagedResult<IslandDto>> GetAllAsync(int page, int pageSize, string? locale = null)
    {
        var query = context.Islands.OrderBy(i => i.Name);

        var total = await query.CountAsync();
        var islands = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = islands.Select(i => ToDto(i, locale));

        return new PagedResult<IslandDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<IslandDto> GetByIdAsync(int id, string? locale = null)
    {
        var island = await context.Islands.FindAsync(id)
            ?? throw new NotFoundException($"Island {id} not found.");

        return ToDto(island, locale);
    }

    public async Task<IslandDto> CreateAsync(CreateIslandDto dto)
    {
        await EnsureUniqueAsync(dto.Name, dto.Slug, excludingId: null);

        var island = new Island
        {
            Name = dto.Name,
            Slug = dto.Slug,
            Description = dto.Description,
            CoordinateX = dto.CoordinateX,
            CoordinateY = dto.CoordinateY,
            CoordinateZ = dto.CoordinateZ,
            RotationY = dto.RotationY,
            Scale = dto.Scale,
            ModelUrl = dto.ModelUrl,
            ThumbnailUrl = dto.ThumbnailUrl,
            IsActive = dto.IsActive,
            Translations = dto.Translations
        };
        context.Islands.Add(island);
        await context.SaveChangesAsync();

        return ToDto(island);
    }

    public async Task<IslandDto> UpdateAsync(int id, UpdateIslandDto dto)
    {
        var island = await context.Islands.FindAsync(id)
            ?? throw new NotFoundException($"Island {id} not found.");

        await EnsureUniqueAsync(dto.Name, dto.Slug, excludingId: id);

        island.Name = dto.Name;
        island.Slug = dto.Slug;
        island.Description = dto.Description;
        island.CoordinateX = dto.CoordinateX;
        island.CoordinateY = dto.CoordinateY;
        island.CoordinateZ = dto.CoordinateZ;
        island.RotationY = dto.RotationY;
        island.Scale = dto.Scale;
        island.ModelUrl = dto.ModelUrl;
        island.ThumbnailUrl = dto.ThumbnailUrl;
        island.IsActive = dto.IsActive;
        island.Translations = dto.Translations;
        await context.SaveChangesAsync();

        return ToDto(island);
    }

    public async Task DeleteAsync(int id)
    {
        var island = await context.Islands
            .Include(i => i.ArcIslands)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new NotFoundException($"Island {id} not found.");

        if (island.ArcIslands.Count > 0)
        {
            throw new ConflictException($"Island {id} is linked to {island.ArcIslands.Count} arc(s) and cannot be deleted.");
        }

        context.Islands.Remove(island);
        await context.SaveChangesAsync();
    }

    // Name and Slug are both unique. Checked here so the caller gets a 409 with a
    // readable message instead of a raw index violation from the database.
    private async Task EnsureUniqueAsync(string name, string slug, int? excludingId)
    {
        var conflict = await context.Islands
            .Where(i => excludingId == null || i.Id != excludingId)
            .Where(i => i.Name == name || i.Slug == slug)
            .FirstOrDefaultAsync();

        if (conflict is not null)
        {
            var field = conflict.Name == name ? $"name '{name}'" : $"slug '{slug}'";
            throw new ConflictException($"Another island already uses the {field}.");
        }
    }

    private static IslandDto ToDto(Island i, string? locale = null) => new(
        i.Id,
        LocaleResolver.Resolve(i.Name, i.Translations, locale, t => t.Name),
        i.Slug,
        LocaleResolver.Resolve(i.Description, i.Translations, locale, t => t.Description),
        i.CoordinateX, i.CoordinateY, i.CoordinateZ,
        i.RotationY, i.Scale, i.ModelUrl, i.ThumbnailUrl, i.IsActive);
}
