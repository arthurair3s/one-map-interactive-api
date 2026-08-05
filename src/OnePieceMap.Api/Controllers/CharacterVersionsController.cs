using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Features.CharacterVersions;

namespace OnePieceMap.Api.Controllers;

[ApiController]
[Route("characters/{characterId:int}/versions")]
public class CharacterVersionsController(
    CharacterVersionService service,
    IValidator<CreateCharacterVersionDto> createValidator) : ControllerBase
{
    /// <summary>Lists all versions of a character, ordered by the arc's GlobalOrder. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CharacterVersionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CharacterVersionDto>>> GetByCharacter(int characterId, [FromQuery] string? locale = null)
        => Ok(await service.GetByCharacterAsync(characterId, locale));

    /// <summary>Creates a version of a character tied to an arc. Only one version per (character, arc) pair is allowed — RN04.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CharacterVersionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CharacterVersionDto>> Create(int characterId, CreateCharacterVersionDto dto)
    {
        if (await this.ValidateAsync(createValidator, dto) is { } invalid)
        {
            return invalid;
        }

        var created = await service.CreateAsync(characterId, dto);
        return CreatedAtAction(nameof(GetByCharacter), new { characterId }, created);
    }
}
