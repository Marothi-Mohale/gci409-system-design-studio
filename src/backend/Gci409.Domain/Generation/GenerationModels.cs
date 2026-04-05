using Gci409.Domain.Artifacts;
using Gci409.Domain.Common;

namespace Gci409.Domain.Generation;

public enum GenerationRequestStatus
{
    Queued = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

public sealed class GenerationRequest : AuditableEntity, IAggregateRoot
{
    private readonly List<GenerationRequestTarget> _targets = [];

    private GenerationRequest()
    {
    }

    public Guid ProjectId { get; private set; }

    public Guid RequirementSetVersionId { get; private set; }

    public GenerationRequestStatus Status { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public string? FailureReason { get; private set; }

    public IReadOnlyCollection<GenerationRequestTarget> Targets => _targets;

    public static GenerationRequest Create(Guid projectId, Guid requirementSetVersionId, IEnumerable<GenerationRequestTarget> targets, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        var request = new GenerationRequest
        {
            ProjectId = projectId,
            RequirementSetVersionId = requirementSetVersionId,
            Status = GenerationRequestStatus.Queued,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };

        foreach (var target in targets)
        {
            target.AttachToRequest(request.Id);
            request._targets.Add(target);
        }

        return request;
    }

    public void MarkProcessing(Guid modifiedByUserId, DateTimeOffset startedAtUtc)
    {
        Status = GenerationRequestStatus.Processing;
        StartedAtUtc = startedAtUtc;
        FailureReason = null;
        Touch(modifiedByUserId, startedAtUtc);
    }

    public void MarkCompleted(Guid modifiedByUserId, DateTimeOffset completedAtUtc)
    {
        Status = GenerationRequestStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        FailureReason = null;
        Touch(modifiedByUserId, completedAtUtc);
    }

    public void MarkFailed(string failureReason, Guid modifiedByUserId, DateTimeOffset failedAtUtc)
    {
        Status = GenerationRequestStatus.Failed;
        CompletedAtUtc = failedAtUtc;
        FailureReason = failureReason;
        Touch(modifiedByUserId, failedAtUtc);
    }
}

public sealed class GenerationRequestTarget : Entity
{
    private GenerationRequestTarget()
    {
    }

    public Guid GenerationRequestId { get; private set; }

    public ArtifactKind ArtifactKind { get; private set; }

    public OutputFormat PreferredFormat { get; private set; }

    public static GenerationRequestTarget Create(ArtifactKind artifactKind, OutputFormat preferredFormat)
    {
        return new GenerationRequestTarget
        {
            ArtifactKind = artifactKind,
            PreferredFormat = preferredFormat
        };
    }

    internal void AttachToRequest(Guid generationRequestId)
    {
        GenerationRequestId = generationRequestId;
    }
}
