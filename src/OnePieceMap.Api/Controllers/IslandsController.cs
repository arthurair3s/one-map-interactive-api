using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Features.Islands;
using OnePieceMap.Application.Features.Wiki;

namespace OnePieceMap.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class IslandsController(
    IslandService service,
    WikiService wikiService,
    IValidator<CreateIslandDto> createValidator,
    IValidator<UpdateIslandDto> updateValidator) : ControllerBase
{
    /// <summary>Lists islands (raw data, no arc context), paginated. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<IslandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? locale = null)
        => Ok(await service.GetAllAsync(page, pageSize, locale));

    /// <summary>Gets a single island by id (raw data, no arc context). Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(IslandDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IslandDto>> GetById(int id, [FromQuery] string? locale = null)
        => Ok(await service.GetByIdAsync(id, locale));

    /// <summary>Gets an island's description, events and present characters (RN05-resolved) for a specific arc — RN08. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet("{id:int}/details")]
    [ProducesResponseType(typeof(IslandDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IslandDetailsDto>> GetDetails(int id, [FromQuery] int? arcId, [FromQuery] string? locale = null)
    {
        if (this.RequireQueryParam(arcId, nameof(arcId)) is { } invalid)
        {
            return invalid;
        }

        return Ok(await wikiService.GetIslandDetailsAsync(id, arcId!.Value, locale));
    }

    /// <summary>Creates an island. Name must be unique.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(IslandDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IslandDto>> Create(CreateIslandDto dto)
    {
        if (await this.ValidateAsync(createValidator, dto) is { } invalid)
        {
            return invalid;
        }

        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an island. Name must stay unique.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(IslandDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IslandDto>> Update(int id, UpdateIslandDto dto)
    {
        if (await this.ValidateAsync(updateValidator, dto) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.UpdateAsync(id, dto));
    }

    /// <summary>Deletes an island. Blocked (409) while it's still linked to any arc — RN01.</summary>
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
