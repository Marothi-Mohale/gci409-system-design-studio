using Gci409.Application.Common;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Recommendations;

namespace Gci409.Infrastructure.Recommendations;

public sealed class RuleBasedArtifactRecommendationEngine : IArtifactRecommendationEngine
{
    private static readonly Dictionary<ArtifactKind, string[]> KeywordMap = new()
    {
        [ArtifactKind.UseCaseDiagram] = ["actor", "user", "role", "journey", "interaction"],
        [ArtifactKind.ClassDiagram] = ["entity", "object", "domain", "relationship", "aggregate"],
        [ArtifactKind.SequenceDiagram] = ["sequence", "step", "interaction", "request", "response", "flow"],
        [ArtifactKind.ActivityDiagram] = ["workflow", "approval", "process", "state", "branch"],
        [ArtifactKind.ComponentDiagram] = ["module", "service", "component", "integration", "boundary"],
        [ArtifactKind.DeploymentDiagram] = ["deploy", "hosting", "infra", "availability", "container", "cloud"],
        [ArtifactKind.ContextDiagram] = ["external", "system", "context", "boundary", "integration"],
        [ArtifactKind.DataFlowDiagram] = ["data flow", "movement", "pipeline", "ingestion", "handoff"],
        [ArtifactKind.Erd] = ["data", "table", "relationship", "schema", "entity"],
        [ArtifactKind.ArchitectureSummary] = ["architecture", "reliability", "scalability", "security"],
        [ArtifactKind.ModuleDecomposition] = ["module", "bounded context", "capability", "responsibility"],
        [ArtifactKind.ApiDesignSuggestion] = ["api", "endpoint", "integration", "contract"],
        [ArtifactKind.DatabaseDesignSuggestion] = ["database", "data", "query", "schema", "storage"]
    };

    public Task<IReadOnlyCollection<ArtifactRecommendationDraft>> RecommendAsync(RecommendationInput input, CancellationToken cancellationToken = default)
    {
        var corpus = string.Join(" ", input.RequirementDescriptions.Concat(input.ConstraintDescriptions).Append(input.RequirementSummary)).ToLowerInvariant();
        var recommendations = new List<ArtifactRecommendationDraft>();

        foreach (var entry in KeywordMap)
        {
            var hits = entry.Value.Count(corpus.Contains);
            if (entry.Key == ArtifactKind.ArchitectureSummary)
            {
                hits = Math.Max(hits, 1);
            }

            if (hits == 0)
            {
                continue;
            }

            var confidence = Math.Min(0.95m, 0.35m + (hits * 0.15m));
            var strength = confidence switch
            {
                >= 0.75m => RecommendationStrength.High,
                >= 0.55m => RecommendationStrength.Medium,
                _ => RecommendationStrength.Low
            };

            recommendations.Add(new ArtifactRecommendationDraft(
                entry.Key,
                $"{entry.Key} recommended",
                BuildRationale(entry.Key, hits),
                confidence,
                strength));
        }

        IReadOnlyCollection<ArtifactRecommendationDraft> results = recommendations
            .OrderByDescending(x => x.ConfidenceScore)
            .ThenBy(x => x.ArtifactKind)
            .ToList();

        return Task.FromResult(results);
    }

    private static string BuildRationale(ArtifactKind artifactKind, int hits)
    {
        return artifactKind switch
        {
            ArtifactKind.UseCaseDiagram => $"The requirements emphasize actors and interactions across the system ({hits} signal matches).",
            ArtifactKind.ClassDiagram => $"The requirements highlight domain structure and relationships ({hits} signal matches).",
            ArtifactKind.SequenceDiagram => $"The requirements describe ordered interactions and request flows ({hits} signal matches).",
            ArtifactKind.ActivityDiagram => $"The requirements emphasize workflows, branching, or process logic ({hits} signal matches).",
            ArtifactKind.ComponentDiagram => $"The requirements highlight modules, services, and integration boundaries ({hits} signal matches).",
            ArtifactKind.DeploymentDiagram => $"The constraints include infrastructure or runtime concerns ({hits} signal matches).",
            ArtifactKind.ContextDiagram => $"The requirements mention external systems or system boundaries ({hits} signal matches).",
            ArtifactKind.DataFlowDiagram => $"The requirements emphasize movement of data across steps or systems ({hits} signal matches).",
            ArtifactKind.Erd => $"The requirements emphasize data entities and their relationships ({hits} signal matches).",
            ArtifactKind.ArchitectureSummary => "Every project benefits from an explicit architecture summary and decision baseline.",
            ArtifactKind.ModuleDecomposition => $"The requirements indicate a need to define responsibilities and modular boundaries ({hits} signal matches).",
            ArtifactKind.ApiDesignSuggestion => $"The requirements point to service contracts or integration interfaces ({hits} signal matches).",
            ArtifactKind.DatabaseDesignSuggestion => $"The requirements indicate meaningful storage and query design concerns ({hits} signal matches).",
            _ => "The project inputs indicate this artifact is relevant."
        };
    }
}
