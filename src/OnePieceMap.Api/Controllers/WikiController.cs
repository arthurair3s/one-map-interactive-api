using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Features.Wiki;

namespace OnePieceMap.Api.Controllers;

// Read-only endpoints optimized for the frontend map (project_overview.md §9).
[ApiController]
[Route("wiki")]
public class WikiController(WikiService service) : ControllerBase
{
    /// <summary>Lean saga list ({id, name, order}) for the saga selector.</summary>
    [HttpGet("sagas")]
    [ProducesResponseType(typeof(IEnumerable<WikiSagaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WikiSagaDto>>> GetSagas()
        => Ok(await service.GetSagasAsync());

    /// <summary>Lean arc list ({id, name, order}) for the arc selector, optionally filtered by saga.</summary>
    [HttpGet("arcs")]
    [ProducesResponseType(typeof(IEnumerable<WikiArcDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WikiArcDto>>> GetArcs([FromQuery] int? sagaId = null)
        => Ok(await service.GetArcsAsync(sagaId));

    /// <summary>All islands ever revealed, each with FirstAppearanceGlobalOrder for client-side timeline filtering — RN11.</summary>
    [HttpGet("map")]
    [ProducesResponseType(typeof(IEnumerable<WikiMapItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WikiMapItemDto>>> GetMap()
        => Ok(await service.GetMapAsync());

    /// <summary>Same as GET /islands/{id}/details, filtered and formatted for direct frontend consumption — RN08.</summary>
    [HttpGet("islands/{id:int}/details")]
    [ProducesResponseType(typeof(IslandDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IslandDetailsDto>> GetIslandDetails(int id, [FromQuery] int? arcId)
    {
        if (this.RequireQueryParam(arcId, nameof(arcId)) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.GetIslandDetailsAsync(id, arcId!.Value));
    }
}
