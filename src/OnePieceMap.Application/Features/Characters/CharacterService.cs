using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.Characters;

public class CharacterService(AppDbContext context)
{
    public async Task<PagedResult<CharacterDto>> GetAllAsync(int page, int pageSize)
    {
        var query = context.Characters.OrderBy(c => c.Name);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CharacterDto(c.Id, c.Name, c.Slug))
            .ToListAsync();

        return new PagedResult<CharacterDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<CharacterDto> GetByIdAsync(int id)
    {
        var character = await context.Characters.FindAsync(id)
            ?? throw new NotFoundException($"Character {id} not found.");

        return new CharacterDto(character.Id, character.Name, character.Slug);
    }

    public async Task<CharacterDto> CreateAsync(CreateCharacterDto dto)
    {
        await EnsureUniqueSlugAsync(dto.Slug, excludingId: null);

        var character = new Character { Name = dto.Name, Slug = dto.Slug };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        return new CharacterDto(character.Id, character.Name, character.Slug);
    }

    public async Task<CharacterDto> UpdateAsync(int id, UpdateCharacterDto dto)
    {
        var character = await context.Characters.FindAsync(id)
            ?? throw new NotFoundException($"Character {id} not found.");

        await EnsureUniqueSlugAsync(dto.Slug, excludingId: id);

        character.Name = dto.Name;
        character.Slug = dto.Slug;
        await context.SaveChangesAsync();

        return new CharacterDto(character.Id, character.Name, character.Slug);
    }

    public async Task DeleteAsync(int id)
    {
        var character = await context.Characters
            .Include(c => c.Versions)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new NotFoundException($"Character {id} not found.");

        if (character.Versions.Count > 0)
        {
            throw new ConflictException($"Character {id} has {character.Versions.Count} version(s) and cannot be deleted.");
        }

        context.Characters.Remove(character);
        await context.SaveChangesAsync();
    }

    private async Task EnsureUniqueSlugAsync(string slug, int? excludingId)
    {
        var conflict = await context.Characters
            .Where(c => excludingId == null || c.Id != excludingId)
            .AnyAsync(c => c.Slug == slug);

        if (conflict)
        {
            throw new ConflictException($"Another character already uses the slug '{slug}'.");
        }
    }
}
