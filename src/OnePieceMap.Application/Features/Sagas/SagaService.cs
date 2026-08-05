using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.Sagas;

public class SagaService(AppDbContext context)
{
    public async Task<PagedResult<SagaDto>> GetAllAsync(int page, int pageSize, string? locale = null)
    {
        var query = context.Sagas.OrderBy(s => s.Order);

        var total = await query.CountAsync();
        var sagas = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = sagas.Select(s => ToDto(s, locale));

        return new PagedResult<SagaDto> { Items = items, Total = total, Page = page, PageSize = pageSize };
    }

    public async Task<SagaDto> GetByIdAsync(int id, string? locale = null)
    {
        var saga = await context.Sagas.FindAsync(id)
            ?? throw new NotFoundException($"Saga {id} not found.");

        return ToDto(saga, locale);
    }

    public async Task<SagaDto> CreateAsync(CreateSagaDto dto)
    {
        await EnsureUniqueAsync(dto.Name, dto.Order, excludingId: null);

        var saga = new Saga { Name = dto.Name, Order = dto.Order, Translations = dto.Translations };
        context.Sagas.Add(saga);
        await context.SaveChangesAsync();

        return ToDto(saga);
    }

    public async Task<SagaDto> UpdateAsync(int id, UpdateSagaDto dto)
    {
        var saga = await context.Sagas.FindAsync(id)
            ?? throw new NotFoundException($"Saga {id} not found.");

        await EnsureUniqueAsync(dto.Name, dto.Order, excludingId: id);

        saga.Name = dto.Name;
        saga.Order = dto.Order;
        saga.Translations = dto.Translations;
        await context.SaveChangesAsync();

        return ToDto(saga);
    }

    public async Task DeleteAsync(int id)
    {
        var saga = await context.Sagas
            .Include(s => s.Arcs)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new NotFoundException($"Saga {id} not found.");

        if (saga.Arcs.Count > 0)
        {
            throw new ConflictException($"Saga {id} has {saga.Arcs.Count} arc(s) and cannot be deleted.");
        }

        context.Sagas.Remove(saga);
        await context.SaveChangesAsync();
    }

    private async Task EnsureUniqueAsync(string name, int order, int? excludingId)
    {
        var conflict = await context.Sagas
            .Where(s => excludingId == null || s.Id != excludingId)
            .Where(s => s.Name == name || s.Order == order)
            .FirstOrDefaultAsync();

        if (conflict is not null)
        {
            var field = conflict.Name == name ? "Name" : "Order";
            throw new ConflictException($"Another saga already uses this {field}.");
        }
    }

    private static SagaDto ToDto(Saga s, string? locale = null) => new(
        s.Id, LocaleResolver.Resolve(s.Name, s.Translations, locale, t => t.Name), s.Order);
}
