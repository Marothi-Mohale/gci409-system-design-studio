using Gci409.Api.Infrastructure;
using Gci409.Application.Admin;
using Gci409.Application.Common;
using Gci409.Domain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "PlatformAdmin")]
public sealed class AdminUsersController(AdminService adminService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<AdminUserSummaryResponse>>> ListUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] UserStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var response = await adminService.GetUsersAsync(User.GetUserId(), Math.Max(page, 1), Math.Clamp(pageSize, 1, 100), q, status, cancellationToken);
        return Ok(response);
    }

    [HttpPatch("users/{userId:guid}")]
    public async Task<ActionResult<AdminUserSummaryResponse>> UpdateUserStatus(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var response = await adminService.UpdateUserStatusAsync(User.GetUserId(), userId, request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyCollection<RoleSummaryResponse>>> ListRoles(CancellationToken cancellationToken)
    {
        var response = await adminService.GetRolesAsync(User.GetUserId(), cancellationToken);
        return Ok(response);
    }
}
