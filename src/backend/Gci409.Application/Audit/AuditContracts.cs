namespace Gci409.Application.Audit;

public sealed record AuditLogResponse(
    Guid Id,
    Guid? ActorUserId,
    Guid? ProjectId,
    string Action,
    string EntityType,
    string EntityId,
    string Description,
    string? CorrelationId,
    string? MetadataJson,
    DateTimeOffset CreatedAtUtc);
