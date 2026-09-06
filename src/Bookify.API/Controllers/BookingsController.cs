using Bookify.Domain.Entities;
using Bookify.Infrastructure.Services.Crud;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers;

[ApiController]
[Route("api/v1/bookings")]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Booking>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Booking>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var booking = await _service.GetByIdAsync(id, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpPost]
    public async Task<ActionResult<Booking>> Create([FromBody] BookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Booking>> Update(Guid id, [FromBody] BookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await _service.UpdateAsync(id, request, cancellationToken);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await _service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}