using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Features.Arcs;

namespace OnePieceMap.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ArcsController(
    ArcService service,
    IValidator<CreateArcDto> createValidator,
    IValidator<UpdateArcDto> updateValidator) : ControllerBase
{
    /// <summary>Lists arcs ordered by GlobalOrder, optionally filtered by saga.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ArcDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ArcDto>>> GetAll([FromQuery] int? sagaId = null)
        => Ok(await service.GetAllAsync(sagaId));

    /// <summary>Gets a single arc by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ArcDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArcDto>> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    /// <summary>Creates an arc under an existing saga. Order (within the saga) and GlobalOrder must both be unique — RN02.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ArcDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArcDto>> Create(CreateArcDto dto)
    {
        if (await this.ValidateAsync(createValidator, dto) is { } invalid)
        {
            return invalid;
        }

        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an arc. Order (within the saga) and GlobalOrder must both stay unique — RN02.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ArcDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ArcDto>> Update(int id, UpdateArcDto dto)
    {
        if (await this.ValidateAsync(updateValidator, dto) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.UpdateAsync(id, dto));
    }

    /// <summary>Deletes an arc. Blocked (409) while it still has linked islands or character versions — RN01.</summary>
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
