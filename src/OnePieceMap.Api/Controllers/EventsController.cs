using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Features.Events;

namespace OnePieceMap.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class EventsController(
    EventService service,
    IValidator<CreateEventDto> createValidator,
    IValidator<UpdateEventDto> updateValidator) : ControllerBase
{
    /// <summary>Lists events for a given arc-island pair, ordered by their order within it. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll([FromQuery] int? arcIslandId, [FromQuery] string? locale = null)
    {
        if (this.RequireQueryParam(arcIslandId, nameof(arcIslandId)) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.GetAllAsync(arcIslandId!.Value, locale));
    }

    /// <summary>Gets an event's detail, including participant character-version ids. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventDetailDto>> GetById(int id, [FromQuery] string? locale = null)
        => Ok(await service.GetByIdAsync(id, locale));

    /// <summary>Creates an event under an existing arc-island pair — RN07. Order must be unique within it — RN03.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventDto>> Create(CreateEventDto dto)
    {
        if (await this.ValidateAsync(createValidator, dto) is { } invalid)
        {
            return invalid;
        }

        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an event. Order must stay unique within its arc-island pair — RN03.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(EventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EventDto>> Update(int id, UpdateEventDto dto)
    {
        if (await this.ValidateAsync(updateValidator, dto) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.UpdateAsync(id, dto));
    }

    /// <summary>Deletes an event. Its participants are removed along with it (Cascade).</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
