using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Features.Characters;

namespace OnePieceMap.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CharactersController(
    CharacterService service,
    IValidator<CreateCharacterDto> createValidator,
    IValidator<UpdateCharacterDto> updateValidator) : ControllerBase
{
    /// <summary>Lists characters, paginated.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CharacterDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await service.GetAllAsync(page, pageSize));

    /// <summary>Gets a single character's base data (name, slug) — not version-specific.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CharacterDto>> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    /// <summary>Creates a character. Slug must be unique.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CharacterDto>> Create(CreateCharacterDto dto)
    {
        if (await this.ValidateAsync(createValidator, dto) is { } invalid)
        {
            return invalid;
        }

        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates a character. Slug must stay unique.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CharacterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CharacterDto>> Update(int id, UpdateCharacterDto dto)
    {
        if (await this.ValidateAsync(updateValidator, dto) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.UpdateAsync(id, dto));
    }

    /// <summary>Deletes a character. Blocked (409) while it still has versions.</summary>
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
