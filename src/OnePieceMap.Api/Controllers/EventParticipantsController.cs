using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Features.EventParticipants;

namespace OnePieceMap.Api.Controllers;

[ApiController]
[Route("events/{eventId:int}/participants")]
public class EventParticipantsController(
    EventParticipantService service,
    IValidator<CreateEventParticipantDto> createValidator) : ControllerBase
{
    /// <summary>Adds a character version as a participant of an event.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Add(int eventId, CreateEventParticipantDto dto)
    {
        if (await this.ValidateAsync(createValidator, dto) is { } invalid)
        {
            return invalid;
        }

        await service.AddAsync(eventId, dto);
        return NoContent();
    }

    /// <summary>Removes a character version's participation from an event.</summary>
    [HttpDelete("{characterVersionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(int eventId, int characterVersionId)
    {
        await service.RemoveAsync(eventId, characterVersionId);
        return NoContent();
    }
}
