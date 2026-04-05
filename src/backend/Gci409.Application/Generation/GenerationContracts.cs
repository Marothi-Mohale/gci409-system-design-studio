using Gci409.Domain.Artifacts;
using Gci409.Domain.Generation;

namespace Gci409.Application.Generation;

public sealed record QueueGenerationRequest(IReadOnlyCollection<ArtifactKind> ArtifactKinds, OutputFormat PreferredFormat = OutputFormat.Markdown);

public sealed record GenerationRequestResponse(Guid Id, Guid RequirementSetVersionId, GenerationRequestStatus Status, IReadOnlyCollection<GenerationTargetResponse> Targets, DateTimeOffset CreatedAtUtc, DateTimeOffset? CompletedAtUtc, string? FailureReason);

public sealed record GenerationTargetResponse(ArtifactKind ArtifactKind, OutputFormat PreferredFormat);
