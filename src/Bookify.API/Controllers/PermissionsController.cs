using Bookify.Domain.Entities.AccessControl;
using Bookify.Infrastructure.Services.Crud;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers;

[ApiController]
[Route("api/v1/permissions")]
public sealed class PermissionsController : ControllerBase
{
    private readonly IPermissionService _service;

    public PermissionsController(IPermissionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Permission>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Permission>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var permission = await _service.GetByIdAsync(id, cancellationToken);
        return permission is null ? NotFound() : Ok(permission);
    }

    [HttpPost]
    public async Task<ActionResult<Permission>> Create([FromBody] PermissionRequest request, CancellationToken cancellationToken)
    {
        var permission = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = permission.Id }, permission);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Permission>> Update(Guid id, [FromBody] PermissionRequest request, CancellationToken cancellationToken)
    {
        var permission = await _service.UpdateAsync(id, request, cancellationToken);
        return permission is null ? NotFound() : Ok(permission);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await _service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}