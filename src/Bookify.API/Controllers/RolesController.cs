using Bookify.Domain.Entities.AccessControl;
using Bookify.Infrastructure.Services.Crud;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Role>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Role>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var role = await _service.GetByIdAsync(id, cancellationToken);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpPost]
    public async Task<ActionResult<Role>> Create([FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = role.Id }, role);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Role>> Update(Guid id, [FromBody] RoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _service.UpdateAsync(id, request, cancellationToken);
        return role is null ? NotFound() : Ok(role);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => await _service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}