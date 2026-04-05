using Gci409.Application.Common;
using Gci409.Infrastructure.OpenAi;
using Microsoft.Extensions.Logging;

namespace Gci409.Infrastructure.Recommendations;

internal sealed class HybridArtifactRecommendationEngine(
    IOpenAiJsonClient openAiJsonClient,
    OpenAiArtifactRecommendationEngine openAiEngine,
    RuleBasedArtifactRecommendationEngine fallbackEngine,
    ILogger<HybridArtifactRecommendationEngine> logger) : IArtifactRecommendationEngine
{
    public async Task<IReadOnlyCollection<ArtifactRecommendationDraft>> RecommendAsync(RecommendationInput input, CancellationToken cancellationToken = default)
    {
        if (!openAiJsonClient.IsConfigured)
        {
            return await fallbackEngine.RecommendAsync(input, cancellationToken);
        }

        try
        {
            return await openAiEngine.RecommendAsync(input, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falling back to rule-based recommendations after OpenAI recommendation failure.");
            return await fallbackEngine.RecommendAsync(input, cancellationToken);
        }
    }
}
