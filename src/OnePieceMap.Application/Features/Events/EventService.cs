using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Enums;
using OnePieceMap.Infrastructure.Data;
using Event = OnePieceMap.Domain.Entities.Event;

namespace OnePieceMap.Application.Features.Events;

public class EventService(AppDbContext context)
{
    public async Task<IEnumerable<EventDto>> GetAllAsync(int arcIslandId)
    {
        await EnsureArcIslandExistsAsync(arcIslandId);

        var events = await context.Events
            .Where(e => e.ArcIslandId == arcIslandId)
            .OrderBy(e => e.Order)
            .ToListAsync();

        return events.Select(ToDto);
    }

    public async Task<EventDetailDto> GetByIdAsync(int id)
    {
        var ev = await context.Events
            .Include(e => e.Participants)
            .FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException($"Event {id} not found.");

        return new EventDetailDto(
            ev.Id, ev.ArcIslandId, ev.Title, ev.Description, ev.Type.ToString(), ev.Order,
            ev.Participants.Select(p => p.CharacterVersionId));
    }

    // RN07: ArcIslandId must reference an existing arc-island pair.
    // RN03: unique Order within the ArcIsland.
    public async Task<EventDto> CreateAsync(CreateEventDto dto)
    {
        await EnsureArcIslandExistsAsync(dto.ArcIslandId);
        await EnsureUniqueOrderAsync(dto.ArcIslandId, dto.Order, excludingId: null);

        var ev = new Event
        {
            ArcIslandId = dto.ArcIslandId,
            Title = dto.Title,
            Description = dto.Description,
            Type = ParseType(dto.Type),
            Order = dto.Order
        };
        context.Events.Add(ev);
        await context.SaveChangesAsync();

        return ToDto(ev);
    }

    public async Task<EventDto> UpdateAsync(int id, UpdateEventDto dto)
    {
        var ev = await context.Events.FindAsync(id)
            ?? throw new NotFoundException($"Event {id} not found.");

        await EnsureArcIslandExistsAsync(dto.ArcIslandId);
        await EnsureUniqueOrderAsync(dto.ArcIslandId, dto.Order, excludingId: id);

        ev.ArcIslandId = dto.ArcIslandId;
        ev.Title = dto.Title;
        ev.Description = dto.Description;
        ev.Type = ParseType(dto.Type);
        ev.Order = dto.Order;
        await context.SaveChangesAsync();

        return ToDto(ev);
    }

    public async Task DeleteAsync(int id)
    {
        var ev = await context.Events.FindAsync(id)
            ?? throw new NotFoundException($"Event {id} not found.");

        // EventParticipant -> Event uses Cascade (§3.2), so no delete-block needed here.
        context.Events.Remove(ev);
        await context.SaveChangesAsync();
    }

    private async Task EnsureArcIslandExistsAsync(int arcIslandId)
    {
        var exists = await context.ArcIslands.AnyAsync(ai => ai.Id == arcIslandId);
        if (!exists)
        {
            throw new NotFoundException($"ArcIsland {arcIslandId} not found.");
        }
    }

    private async Task EnsureUniqueOrderAsync(int arcIslandId, int order, int? excludingId)
    {
        var conflict = await context.Events
            .Where(e => excludingId == null || e.Id != excludingId)
            .AnyAsync(e => e.ArcIslandId == arcIslandId && e.Order == order);

        if (conflict)
        {
            throw new ConflictException($"Another event already uses order {order} within this arc-island.");
        }
    }

    private static EventType ParseType(string type) => Enum.Parse<EventType>(type, ignoreCase: true);

    private static EventDto ToDto(Event e) => new(e.Id, e.ArcIslandId, e.Title, e.Description, e.Type.ToString(), e.Order);
}
