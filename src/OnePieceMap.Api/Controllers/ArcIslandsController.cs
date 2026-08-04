using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Features.ArcIslands;

namespace OnePieceMap.Api.Controllers;

[ApiController]
[Route("arcs/{arcId:int}/islands")]
public class ArcIslandsController(
    ArcIslandService service,
    IValidator<CreateArcIslandDto> createValidator) : ControllerBase
{
    /// <summary>Lists the islands linked to an arc, ordered by route order — RN06.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ArcIslandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ArcIslandDto>>> GetByArc(int arcId)
        => Ok(await service.GetByArcAsync(arcId));

    /// <summary>Links an island to an arc with a route order. The (arc, island) pair and the order must both be unique.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ArcIslandDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArcIslandDto>> Create(int arcId, CreateArcIslandDto dto)
    {
        if (await this.ValidateAsync(createValidator, dto) is { } invalid)
        {
            return invalid;
        }

        var created = await service.CreateAsync(arcId, dto);
        return CreatedAtAction(nameof(GetByArc), new { arcId }, created);
    }

    /// <summary>Removes the link between an arc and an island. Blocked (409) while it still has events.</summary>
    [HttpDelete("{islandId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int arcId, int islandId)
    {
        await service.DeleteAsync(arcId, islandId);
        return NoContent();
    }
}
