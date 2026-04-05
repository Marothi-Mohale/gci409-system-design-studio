using Gci409.Domain.Common;

namespace Gci409.Domain.Audit;

public sealed class AuditLog : AuditableEntity, IAggregateRoot
{
    private AuditLog()
    {
    }

    public Guid? ActorUserId { get; private set; }

    public Guid? ProjectId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string EntityType { get; private set; } = string.Empty;

    public string EntityId { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string? CorrelationId { get; private set; }

    public string? MetadataJson { get; private set; }

    public static AuditLog Create(
        Guid? actorUserId,
        Guid? projectId,
        string action,
        string entityType,
        string entityId,
        string description,
        string? correlationId,
        string? metadataJson,
        DateTimeOffset occurredAtUtc)
    {
        return new AuditLog
        {
            ActorUserId = actorUserId,
            ProjectId = projectId,
            Action = action.Trim(),
            EntityType = entityType.Trim(),
            EntityId = entityId.Trim(),
            Description = description.Trim(),
            CorrelationId = correlationId,
            MetadataJson = metadataJson,
            CreatedByUserId = actorUserId,
            CreatedAtUtc = occurredAtUtc
        };
    }
}
