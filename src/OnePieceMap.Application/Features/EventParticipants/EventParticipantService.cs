using Microsoft.EntityFrameworkCore;
using OnePieceMap.Application.Common.Exceptions;
using OnePieceMap.Domain.Entities;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Application.Features.EventParticipants;

public class EventParticipantService(AppDbContext context)
{
    public async Task AddAsync(int eventId, CreateEventParticipantDto dto)
    {
        await EnsureEventExistsAsync(eventId);

        var versionExists = await context.CharacterVersions.AnyAsync(cv => cv.Id == dto.CharacterVersionId);
        if (!versionExists)
        {
            throw new NotFoundException($"CharacterVersion {dto.CharacterVersionId} not found.");
        }

        var alreadyExists = await context.EventParticipants
            .AnyAsync(ep => ep.EventId == eventId && ep.CharacterVersionId == dto.CharacterVersionId);
        if (alreadyExists)
        {
            throw new ConflictException("This character version is already a participant of this event.");
        }

        context.EventParticipants.Add(new EventParticipant { EventId = eventId, CharacterVersionId = dto.CharacterVersionId });
        await context.SaveChangesAsync();
    }

    public async Task RemoveAsync(int eventId, int characterVersionId)
    {
        var participant = await context.EventParticipants
            .FirstOrDefaultAsync(ep => ep.EventId == eventId && ep.CharacterVersionId == characterVersionId)
            ?? throw new NotFoundException("Participant link not found.");

        context.EventParticipants.Remove(participant);
        await context.SaveChangesAsync();
    }

    private async Task EnsureEventExistsAsync(int eventId)
    {
        var exists = await context.Events.AnyAsync(e => e.Id == eventId);
        if (!exists)
        {
            throw new NotFoundException($"Event {eventId} not found.");
        }
    }
}
