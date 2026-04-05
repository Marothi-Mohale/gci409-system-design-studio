using Gci409.Application.Common;
using Gci409.Domain.Audit;

namespace Gci409.Infrastructure.Persistence;

public sealed class AuditWriter(IGci409DbContext dbContext, IClock clock) : IAuditWriter
{
    public async Task WriteAsync(
        Guid? actorUserId,
        Guid? projectId,
        string action,
        string entityType,
        string entityId,
        string description,
        string? metadataJson = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = AuditLog.Create(actorUserId, projectId, action, entityType, entityId, description, null, metadataJson, clock.UtcNow);
        await dbContext.AuditLogs.AddAsync(auditLog, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
