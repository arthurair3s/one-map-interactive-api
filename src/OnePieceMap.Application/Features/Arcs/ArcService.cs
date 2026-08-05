using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.Arcs;

public class ArcService(AppDbContext context)
{
    public async Task<IEnumerable<ArcDto>> GetAllAsync(int? sagaId, string? locale = null)
    {
        var query = context.Arcs.AsQueryable();

        if (sagaId is not null)
        {
            query = query.Where(a => a.SagaId == sagaId);
        }

        var arcs = await query.OrderBy(a => a.GlobalOrder).ToListAsync();

        return arcs.Select(a => ToDto(a, locale));
    }

    public async Task<ArcDto> GetByIdAsync(int id, string? locale = null)
    {
        var arc = await context.Arcs.FindAsync(id)
            ?? throw new NotFoundException($"Arc {id} not found.");

        return ToDto(arc, locale);
    }

    public async Task<ArcDto> CreateAsync(CreateArcDto dto)
    {
        await EnsureSagaExistsAsync(dto.SagaId);
        await EnsureUniqueAsync(dto.SagaId, dto.Order, dto.GlobalOrder, excludingId: null);

        var arc = new Arc
        {
            SagaId = dto.SagaId,
            Name = dto.Name,
            Order = dto.Order,
            GlobalOrder = dto.GlobalOrder,
            Translations = dto.Translations
        };
        context.Arcs.Add(arc);
        await context.SaveChangesAsync();

        return ToDto(arc);
    }

    public async Task<ArcDto> UpdateAsync(int id, UpdateArcDto dto)
    {
        var arc = await context.Arcs.FindAsync(id)
            ?? throw new NotFoundException($"Arc {id} not found.");

        await EnsureSagaExistsAsync(dto.SagaId);
        await EnsureUniqueAsync(dto.SagaId, dto.Order, dto.GlobalOrder, excludingId: id);

        arc.SagaId = dto.SagaId;
        arc.Name = dto.Name;
        arc.Order = dto.Order;
        arc.GlobalOrder = dto.GlobalOrder;
        arc.Translations = dto.Translations;
        await context.SaveChangesAsync();

        return ToDto(arc);
    }

    public async Task DeleteAsync(int id)
    {
        var arc = await context.Arcs
            .Include(a => a.ArcIslands)
            .Include(a => a.CharacterVersions)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException($"Arc {id} not found.");

        if (arc.ArcIslands.Count > 0)
        {
            throw new ConflictException($"Arc {id} has {arc.ArcIslands.Count} linked island(s) and cannot be deleted.");
        }

        if (arc.CharacterVersions.Count > 0)
        {
            throw new ConflictException($"Arc {id} has {arc.CharacterVersions.Count} character version(s) and cannot be deleted.");
        }

        context.Arcs.Remove(arc);
        await context.SaveChangesAsync();
    }

    private async Task EnsureSagaExistsAsync(int sagaId)
    {
        var exists = await context.Sagas.AnyAsync(s => s.Id == sagaId);
        if (!exists)
        {
            throw new NotFoundException($"Saga {sagaId} not found.");
        }
    }

    // RN02: unique Order within the saga; GlobalOrder unique across all sagas.
    private async Task EnsureUniqueAsync(int sagaId, int order, int globalOrder, int? excludingId)
    {
        var conflict = await context.Arcs
            .Where(a => excludingId == null || a.Id != excludingId)
            .Where(a => (a.SagaId == sagaId && a.Order == order) || a.GlobalOrder == globalOrder)
            .FirstOrDefaultAsync();

        if (conflict is not null)
        {
            var field = conflict.SagaId == sagaId && conflict.Order == order ? "Order (within this saga)" : "GlobalOrder";
            throw new ConflictException($"Another arc already uses this {field}.");
        }
    }

    private static ArcDto ToDto(Arc a, string? locale = null) => new(
        a.Id, a.SagaId, LocaleResolver.Resolve(a.Name, a.Translations, locale, t => t.Name), a.Order, a.GlobalOrder);
}
