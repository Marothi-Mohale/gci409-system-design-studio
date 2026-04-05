namespace Gci409.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTimeOffset CreatedAtUtc { get; protected set; } = DateTimeOffset.UtcNow;

    public Guid? CreatedByUserId { get; protected set; }

    public DateTimeOffset? LastModifiedAtUtc { get; protected set; }

    public Guid? LastModifiedByUserId { get; protected set; }

    public void SetCreated(Guid? userId, DateTimeOffset createdAtUtc)
    {
        CreatedByUserId = userId;
        CreatedAtUtc = createdAtUtc;
    }

    public void Touch(Guid? userId, DateTimeOffset modifiedAtUtc)
    {
        LastModifiedByUserId = userId;
        LastModifiedAtUtc = modifiedAtUtc;
    }
}
