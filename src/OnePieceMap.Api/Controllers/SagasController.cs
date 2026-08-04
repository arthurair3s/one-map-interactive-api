using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OnePieceMap.Api.Extensions;
using OnePieceMap.Application.Common;
using OnePieceMap.Application.Features.Sagas;

namespace OnePieceMap.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class SagasController(
    SagaService service,
    IValidator<CreateSagaDto> createValidator,
    IValidator<UpdateSagaDto> updateValidator) : ControllerBase
{
    /// <summary>Lists sagas, ordered by their display order.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SagaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await service.GetAllAsync(page, pageSize));

    /// <summary>Gets a single saga by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SagaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SagaDto>> GetById(int id)
        => Ok(await service.GetByIdAsync(id));

    /// <summary>Creates a saga. Name and Order must both be unique across sagas.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SagaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SagaDto>> Create(CreateSagaDto dto)
    {
        if (await this.ValidateAsync(createValidator, dto) is { } invalid)
        {
            return invalid;
        }

        var created = await service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates a saga. Name and Order must both stay unique across sagas.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SagaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SagaDto>> Update(int id, UpdateSagaDto dto)
    {
        if (await this.ValidateAsync(updateValidator, dto) is { } invalid)
        {
            return invalid;
        }

        return Ok(await service.UpdateAsync(id, dto));
    }

    /// <summary>Deletes a saga. Blocked (409) while it still has arcs — RN01.</summary>
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
