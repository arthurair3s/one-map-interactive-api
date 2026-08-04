using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Features.CharacterVersions;

namespace OnePieceMap.Api.Controllers;

// Flat routes by CharacterVersion id, per the API contract (§5.2) — separate from the
// nested characters/{characterId}/versions routes in CharacterVersionsController.
[ApiController]
[Route("character-versions")]
public class CharacterVersionController(
    CharacterVersionService service,
    IValidator<UpdateCharacterVersionDto> updateValidator) : ControllerBase
{
    /// <summary>Updates a character version. Its arc must stay unique per (character, arc) pair — RN04.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CharacterVersionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CharacterVersionDto>> Update(int id, UpdateCharacterVersionDto dto)
    {
        if (await this.ValidateAsync(updateValidator, dto) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.UpdateAsync(id, dto));
    }

    /// <summary>Deletes a character version. Blocked (409) while it still has event participations.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
