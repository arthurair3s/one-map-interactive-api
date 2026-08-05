using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Features.Wiki;

namespace OnePieceMap.Api.Controllers;

// Read-only endpoints optimized for the frontend map (project_overview.md §9).
[ApiController]
[Route("wiki")]
public class WikiController(WikiService service) : ControllerBase
{
    /// <summary>Lean saga list ({id, name, order}) for the saga selector. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet("sagas")]
    [ProducesResponseType(typeof(IEnumerable<WikiSagaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WikiSagaDto>>> GetSagas([FromQuery] string? locale = null)
        => Ok(await service.GetSagasAsync(locale));

    /// <summary>Lean arc list ({id, name, order}) for the arc selector, optionally filtered by saga. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet("arcs")]
    [ProducesResponseType(typeof(IEnumerable<WikiArcDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WikiArcDto>>> GetArcs([FromQuery] int? sagaId = null, [FromQuery] string? locale = null)
        => Ok(await service.GetArcsAsync(sagaId, locale));

    /// <summary>All islands ever revealed, each with FirstAppearanceGlobalOrder for client-side timeline filtering — RN11. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet("map")]
    [ProducesResponseType(typeof(IEnumerable<WikiMapItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<WikiMapItemDto>>> GetMap([FromQuery] string? locale = null)
        => Ok(await service.GetMapAsync(locale));

    /// <summary>Same as GET /islands/{id}/details, filtered and formatted for direct frontend consumption — RN08. Accepts <c>?locale=</c> (default <c>en</c>) — RN12.</summary>
    [HttpGet("islands/{id:int}/details")]
    [ProducesResponseType(typeof(IslandDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IslandDetailsDto>> GetIslandDetails(int id, [FromQuery] int? arcId, [FromQuery] string? locale = null)
    {
        if (this.RequireQueryParam(arcId, nameof(arcId)) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.GetIslandDetailsAsync(id, arcId!.Value, locale));
    }
}
