using Gci409.Application.Common;
using Gci409.Application.Admin;
using Gci409.Application.Projects;
using Gci409.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Audit;

public sealed class AuditService(
    IGci409DbContext dbContext,
    ProjectService projectService,
    AdminService adminService)
{
    public async Task<PagedResult<AuditLogResponse>> GetProjectAuditLogsAsync(Guid projectId, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        var query = dbContext.AuditLogs
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapExpression())
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogResponse>(items, page, pageSize, totalCount);
    }

    public async Task<PagedResult<AuditLogResponse>> GetPlatformAuditLogsAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await adminService.EnsurePlatformAdministratorAsync(userId, cancellationToken);

        var query = dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapExpression())
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogResponse>(items, page, pageSize, totalCount);
    }

    private static System.Linq.Expressions.Expression<Func<Domain.Audit.AuditLog, AuditLogResponse>> MapExpression()
    {
        return x => new AuditLogResponse(x.Id, x.ActorUserId, x.ProjectId, x.Action, x.EntityType, x.EntityId, x.Description, x.CorrelationId, x.MetadataJson, x.CreatedAtUtc);
    }
}
