using Gci409.Domain.Common;

namespace Gci409.Domain.Recommendations;

public enum RecommendationStrength
{
    Low = 1,
    Medium = 2,
    High = 3
}

public sealed class RecommendationSet : AuditableEntity, IAggregateRoot
{
    private readonly List<Recommendation> _items = [];

    private RecommendationSet()
    {
    }

    public Guid ProjectId { get; private set; }

    public Guid RequirementSetVersionId { get; private set; }

    public IReadOnlyCollection<Recommendation> Items => _items;

    public static RecommendationSet Create(Guid projectId, Guid requirementSetVersionId, IEnumerable<Recommendation> items, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        var set = new RecommendationSet
        {
            ProjectId = projectId,
            RequirementSetVersionId = requirementSetVersionId,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };

        foreach (var item in items)
        {
            item.AttachToSet(set.Id);
            set._items.Add(item);
        }

        return set;
    }
}

public sealed class Recommendation : Entity
{
    private Recommendation()
    {
    }

    public Guid RecommendationSetId { get; private set; }

    public int ArtifactKind { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Rationale { get; private set; } = string.Empty;

    public decimal ConfidenceScore { get; private set; }

    public RecommendationStrength Strength { get; private set; }

    public static Recommendation Create(int artifactKind, string title, string rationale, decimal confidenceScore, RecommendationStrength strength)
    {
        return new Recommendation
        {
            ArtifactKind = artifactKind,
            Title = title.Trim(),
            Rationale = rationale.Trim(),
            ConfidenceScore = confidenceScore,
            Strength = strength
        };
    }

    internal void AttachToSet(Guid recommendationSetId)
    {
        RecommendationSetId = recommendationSetId;
    }
}
