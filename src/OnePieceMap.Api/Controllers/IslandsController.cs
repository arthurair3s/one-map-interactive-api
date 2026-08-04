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
    /// <summary>Lists islands (raw data, no arc context), paginated.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<IslandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await service.GetAllAsync(page, pageSize));

    /// <summary>Gets a single island by id (raw data, no arc context).</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(IslandDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IslandDto>> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    /// <summary>Gets an island's description, events and present characters (RN05-resolved) for a specific arc — RN08.</summary>
    [HttpGet("{id:int}/details")]
    [ProducesResponseType(typeof(IslandDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IslandDetailsDto>> GetDetails(int id, [FromQuery] int? arcId)
    {
        if (this.RequireQueryParam(arcId, nameof(arcId)) is { } invalid)
        {
            return invalid;
        }

        return Ok(await wikiService.GetIslandDetailsAsync(id, arcId!.Value));
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
