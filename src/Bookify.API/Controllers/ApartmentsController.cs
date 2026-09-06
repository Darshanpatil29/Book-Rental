using Bookify.Domain.Entities;
using Bookify.Infrastructure.Services.Crud;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers;

/// <summary>
/// Basic CRUD endpoints for apartments.
/// </summary>
[ApiController]
[Route("api/v1/apartments")]
public sealed class ApartmentsController : ControllerBase
{
    private readonly IApartmentService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApartmentsController"/> class.
    /// </summary>
    public ApartmentsController(IApartmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gets all apartments.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Apartment>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    /// <summary>
    /// Gets an apartment by identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Apartment>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var apartment = await _service.GetByIdAsync(id, cancellationToken);
        return apartment is null ? NotFound() : Ok(apartment);
    }

    /// <summary>
    /// Creates a new apartment.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Apartment>> Create([FromBody] ApartmentRequest request, CancellationToken cancellationToken)
    {
        var apartment = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = apartment.Id }, apartment);
    }

    /// <summary>
    /// Updates an apartment.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Apartment>> Update(Guid id, [FromBody] ApartmentRequest request, CancellationToken cancellationToken)
    {
        var apartment = await _service.UpdateAsync(id, request, cancellationToken);
        return apartment is null ? NotFound() : Ok(apartment);
    }

    /// <summary>
    /// Deletes an apartment.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await _service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}