using Gci409.Api.Infrastructure;
using Gci409.Application.Audit;
using Gci409.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Authorize]
public sealed class AuditLogsController(AuditService auditService) : ControllerBase
{
    [HttpGet("api/projects/{projectId:guid}/audit-logs")]
    public async Task<ActionResult<PagedResult<AuditLogResponse>>> GetProjectAuditLogs(
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await auditService.GetProjectAuditLogsAsync(projectId, User.GetUserId(), Math.Max(page, 1), Math.Clamp(pageSize, 1, 100), cancellationToken);
        return Ok(response);
    }

    [HttpGet("api/audit-logs")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<ActionResult<PagedResult<AuditLogResponse>>> GetPlatformAuditLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await auditService.GetPlatformAuditLogsAsync(User.GetUserId(), Math.Max(page, 1), Math.Clamp(pageSize, 1, 100), cancellationToken);
        return Ok(response);
    }
}
