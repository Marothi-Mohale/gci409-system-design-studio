using Gci409.Domain.Common;

namespace Gci409.Domain.Collaboration;

public enum CommentStatus
{
    Open = 1,
    Resolved = 2
}

public enum CommentTargetType
{
    Project = 1,
    RequirementSetVersion = 2,
    RecommendationSet = 3,
    GeneratedArtifact = 4,
    ArtifactVersion = 5
}

public sealed class CommentThread : AuditableEntity, IAggregateRoot
{
    private readonly List<Comment> _comments = [];

    private CommentThread()
    {
    }

    public Guid ProjectId { get; private set; }

    public CommentTargetType TargetType { get; private set; }

    public Guid TargetId { get; private set; }

    public CommentStatus Status { get; private set; }

    public IReadOnlyCollection<Comment> Comments => _comments;

    public static CommentThread Create(Guid projectId, CommentTargetType targetType, Guid targetId, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new CommentThread
        {
            ProjectId = projectId,
            TargetType = targetType,
            TargetId = targetId,
            Status = CommentStatus.Open,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }

    public Comment AddComment(string body, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        var comment = Comment.Create(Id, body, createdByUserId, createdAtUtc);
        _comments.Add(comment);
        Touch(createdByUserId, createdAtUtc);
        return comment;
    }

    public void Resolve(Guid modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Status = CommentStatus.Resolved;
        Touch(modifiedByUserId, modifiedAtUtc);
    }
}

public sealed class Comment : AuditableEntity
{
    private Comment()
    {
    }

    public Guid CommentThreadId { get; private set; }

    public string Body { get; private set; } = string.Empty;

    public static Comment Create(Guid commentThreadId, string body, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new Comment
        {
            CommentThreadId = commentThreadId,
            Body = body.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }
}
