using Gci409.Application.Common;
using Gci409.Domain.Artifacts;
using Gci409.Infrastructure.Recommendations;

namespace Gci409.ApplicationTests;

public sealed class RecommendationEngineTests
{
    private readonly RuleBasedArtifactRecommendationEngine _engine = new();

    [Fact]
    public void Recommend_ShouldPrioritizeInteractionArtifacts_WhenActorSignalsExist()
    {
        var input = new RecommendationInput(
            "Claims Platform",
            "The system supports adjusters and claim handlers working through approval steps.",
            [
                "User actors submit claims and managers approve them.",
                "The workflow requires multiple approval steps and interaction flows."
            ],
            ["The solution must integrate with external policy services."]);

        var results = _engine.Recommend(input);

        Assert.Contains(results, x => x.ArtifactKind == ArtifactKind.UseCaseDiagram);
        Assert.Contains(results, x => x.ArtifactKind == ArtifactKind.ActivityDiagram);
        Assert.Contains(results, x => x.ArtifactKind == ArtifactKind.ArchitectureSummary);
    }
}
