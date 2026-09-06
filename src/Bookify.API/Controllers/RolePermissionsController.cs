using Bookify.Domain.Entities.AccessControl;
using Bookify.Infrastructure.Services.Crud;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers;

[ApiController]
[Route("api/v1/role-permissions")]
public sealed class RolePermissionsController : ControllerBase
{
    private readonly IRolePermissionService _service;

    public RolePermissionsController(IRolePermissionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RolePermission>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(cancellationToken));

    [HttpGet("{roleId:guid}/{permissionId:guid}")]
    public async Task<ActionResult<RolePermission>> GetById(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
    {
        var rolePermission = await _service.GetByIdAsync(roleId, permissionId, cancellationToken);
        return rolePermission is null ? NotFound() : Ok(rolePermission);
    }

    [HttpPost]
    public async Task<ActionResult<RolePermission>> Create([FromBody] RolePermissionRequest request, CancellationToken cancellationToken)
    {
        var rolePermission = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { roleId = rolePermission.RoleId, permissionId = rolePermission.PermissionId }, rolePermission);
    }

    [HttpPut("{roleId:guid}/{permissionId:guid}")]
    public async Task<ActionResult<RolePermission>> Update(Guid roleId, Guid permissionId, [FromBody] RolePermissionRequest request, CancellationToken cancellationToken)
    {
        var rolePermission = await _service.UpdateAsync(roleId, permissionId, request, cancellationToken);
        return rolePermission is null ? NotFound() : Ok(rolePermission);
    }

    [HttpDelete("{roleId:guid}/{permissionId:guid}")]
    public async Task<IActionResult> Delete(Guid roleId, Guid permissionId, CancellationToken cancellationToken)
        => await _service.DeleteAsync(roleId, permissionId, cancellationToken) ? NoContent() : NotFound();
}