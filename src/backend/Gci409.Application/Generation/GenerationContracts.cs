using FluentValidation;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Generation;

namespace Gci409.Application.Generation;

public sealed record QueueGenerationRequest(IReadOnlyCollection<ArtifactKind> ArtifactKinds, OutputFormat PreferredFormat = OutputFormat.Markdown);

public sealed record GenerationRequestResponse(Guid Id, Guid RequirementSetVersionId, GenerationRequestStatus Status, IReadOnlyCollection<GenerationTargetResponse> Targets, DateTimeOffset CreatedAtUtc, DateTimeOffset? CompletedAtUtc, string? FailureReason);

public sealed record GenerationTargetResponse(ArtifactKind ArtifactKind, OutputFormat PreferredFormat);

public sealed class QueueGenerationRequestValidator : AbstractValidator<QueueGenerationRequest>
{
    public QueueGenerationRequestValidator()
    {
        RuleFor(x => x.ArtifactKinds).NotEmpty();
        RuleForEach(x => x.ArtifactKinds).IsInEnum();
        RuleFor(x => x.ArtifactKinds)
            .Must(kinds => kinds.Distinct().Count() == kinds.Count)
            .WithMessage("Artifact kinds must be unique within a generation request.");
        RuleFor(x => x.PreferredFormat).IsInEnum();
    }
}
