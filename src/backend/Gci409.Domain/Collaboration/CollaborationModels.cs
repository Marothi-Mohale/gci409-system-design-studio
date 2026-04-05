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
}

public sealed class Comment : AuditableEntity
{
    private Comment()
    {
    }

    public Guid CommentThreadId { get; private set; }

    public string Body { get; private set; } = string.Empty;
}
