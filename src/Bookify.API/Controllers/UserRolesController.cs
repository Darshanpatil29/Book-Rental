using Bookify.Domain.Entities.AccessControl;
using Bookify.Infrastructure.Services.Crud;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.API.Controllers;

[ApiController]
[Route("api/v1/user-roles")]
public sealed class UserRolesController : ControllerBase
{
    private readonly IUserRoleService _service;

    public UserRolesController(IUserRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRole>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(cancellationToken));

    [HttpGet("{userId:guid}/{roleId:guid}")]
    public async Task<ActionResult<UserRole>> GetById(Guid userId, Guid roleId, CancellationToken cancellationToken)
    {
        var userRole = await _service.GetByIdAsync(userId, roleId, cancellationToken);
        return userRole is null ? NotFound() : Ok(userRole);
    }

    [HttpPost]
    public async Task<ActionResult<UserRole>> Create([FromBody] UserRoleRequest request, CancellationToken cancellationToken)
    {
        var userRole = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { userId = userRole.UserId, roleId = userRole.RoleId }, userRole);
    }

    [HttpPut("{userId:guid}/{roleId:guid}")]
    public async Task<ActionResult<UserRole>> Update(Guid userId, Guid roleId, [FromBody] UserRoleRequest request, CancellationToken cancellationToken)
    {
        var userRole = await _service.UpdateAsync(userId, roleId, request, cancellationToken);
        return userRole is null ? NotFound() : Ok(userRole);
    }

    [HttpDelete("{userId:guid}/{roleId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, Guid roleId, CancellationToken cancellationToken)
        => await _service.DeleteAsync(userId, roleId, cancellationToken) ? NoContent() : NotFound();
}