using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.Islands;

public class IslandService(AppDbContext context)
{
    public async Task<PagedResult<IslandDto>> GetAllAsync(int page, int pageSize)
    {
        var query = context.Islands.OrderBy(i => i.Name);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new IslandDto(
                i.Id, i.Name, i.Description,
                i.CoordinateX, i.CoordinateY, i.CoordinateZ,
                i.RotationY, i.Scale, i.ModelUrl, i.ThumbnailUrl, i.IsActive))
            .ToListAsync();

        return new PagedResult<IslandDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<IslandDto> GetByIdAsync(int id)
    {
        var island = await context.Islands.FindAsync(id)
            ?? throw new NotFoundException($"Island {id} not found.");

        return ToDto(island);
    }

    public async Task<IslandDto> CreateAsync(CreateIslandDto dto)
    {
        await EnsureUniqueNameAsync(dto.Name, excludingId: null);

        var island = new Island
        {
            Name = dto.Name,
            Description = dto.Description,
            CoordinateX = dto.CoordinateX,
            CoordinateY = dto.CoordinateY,
            CoordinateZ = dto.CoordinateZ,
            RotationY = dto.RotationY,
            Scale = dto.Scale,
            ModelUrl = dto.ModelUrl,
            ThumbnailUrl = dto.ThumbnailUrl,
            IsActive = dto.IsActive
        };
        context.Islands.Add(island);
        await context.SaveChangesAsync();

        return ToDto(island);
    }

    public async Task<IslandDto> UpdateAsync(int id, UpdateIslandDto dto)
    {
        var island = await context.Islands.FindAsync(id)
            ?? throw new NotFoundException($"Island {id} not found.");

        await EnsureUniqueNameAsync(dto.Name, excludingId: id);

        island.Name = dto.Name;
        island.Description = dto.Description;
        island.CoordinateX = dto.CoordinateX;
        island.CoordinateY = dto.CoordinateY;
        island.CoordinateZ = dto.CoordinateZ;
        island.RotationY = dto.RotationY;
        island.Scale = dto.Scale;
        island.ModelUrl = dto.ModelUrl;
        island.ThumbnailUrl = dto.ThumbnailUrl;
        island.IsActive = dto.IsActive;
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

    private async Task EnsureUniqueNameAsync(string name, int? excludingId)
    {
        var conflict = await context.Islands
            .Where(i => excludingId == null || i.Id != excludingId)
            .AnyAsync(i => i.Name == name);

        if (conflict)
        {
            throw new ConflictException($"Another island already uses the name '{name}'.");
        }
    }

    private static IslandDto ToDto(Island i) => new(
        i.Id, i.Name, i.Description,
        i.CoordinateX, i.CoordinateY, i.CoordinateZ,
        i.RotationY, i.Scale, i.ModelUrl, i.ThumbnailUrl, i.IsActive);
}
