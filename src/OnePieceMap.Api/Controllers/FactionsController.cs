using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Features.Factions;

namespace OnePieceMap.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class FactionsController(
    FactionService service,
    IValidator<CreateFactionDto> createValidator,
    IValidator<UpdateFactionDto> updateValidator) : ControllerBase
{
    /// <summary>Lists factions. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? locale = null)
        => Ok(await service.GetAllAsync(page, pageSize, locale));

    /// <summary>Gets a single faction by id. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FactionDto>> GetById(int id, [FromQuery] string? locale = null)
        => Ok(await service.GetByIdAsync(id, locale));

    /// <summary>Creates a faction. Name and Slug must both be unique across factions.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(FactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FactionDto>> Create(CreateFactionDto dto)
    {
        if (await this.ValidateAsync(createValidator, dto) is { } invalid)
        {
            return invalid;
        }

        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates a faction. Name and Slug must both stay unique across factions.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(FactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FactionDto>> Update(int id, UpdateFactionDto dto)
    {
        if (await this.ValidateAsync(updateValidator, dto) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.UpdateAsync(id, dto));
    }

    /// <summary>Deletes a faction. Blocked (409) while any CharacterVersion still references it.</summary>
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
