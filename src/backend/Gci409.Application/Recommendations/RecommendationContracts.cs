using Gci409.Domain.Artifacts;
using Gci409.Domain.Recommendations;

namespace Gci409.Application.Recommendations;

public sealed record RecommendationResponse(Guid RecommendationSetId, Guid RequirementSetVersionId, IReadOnlyCollection<RecommendationItemResponse> Items, DateTimeOffset CreatedAtUtc);

public sealed record RecommendationItemResponse(ArtifactKind ArtifactKind, string Title, string Rationale, decimal ConfidenceScore, RecommendationStrength Strength);
