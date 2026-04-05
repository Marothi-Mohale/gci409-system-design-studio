using Gci409.Application.Common;
using Gci409.Infrastructure.OpenAi;
using Microsoft.Extensions.Logging;

namespace Gci409.Infrastructure.Generation;

internal sealed class HybridArtifactGenerationEngine(
    IOpenAiJsonClient openAiJsonClient,
    OpenAiArtifactGenerationEngine openAiEngine,
    RuleBasedArtifactGenerationEngine fallbackEngine,
    ILogger<HybridArtifactGenerationEngine> logger) : IArtifactGenerationEngine
{
    public async Task<IReadOnlyCollection<ArtifactDraft>> GenerateAsync(ArtifactGenerationInput input, CancellationToken cancellationToken = default)
    {
        if (!openAiJsonClient.IsConfigured)
        {
            return await fallbackEngine.GenerateAsync(input, cancellationToken);
        }

        try
        {
            var generated = (await openAiEngine.GenerateAsync(input, cancellationToken)).ToList();
            var missingKinds = input.ArtifactKinds
                .Distinct()
                .Except(generated.Select(x => x.ArtifactKind))
                .ToList();

            if (missingKinds.Count == 0)
            {
                return generated;
            }

            logger.LogWarning(
                "OpenAI returned {GeneratedCount} artifacts, but {MissingCount} requested artifact kinds were missing. Completing the result set with the local generator.",
                generated.Count,
                missingKinds.Count);

            var fallbackArtifacts = await fallbackEngine.GenerateAsync(
                input with { ArtifactKinds = missingKinds },
                cancellationToken);

            generated.AddRange(fallbackArtifacts);
            return generated;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falling back to rule-based artifact generation after OpenAI generation failure.");
            return await fallbackEngine.GenerateAsync(input, cancellationToken);
        }
    }
}
