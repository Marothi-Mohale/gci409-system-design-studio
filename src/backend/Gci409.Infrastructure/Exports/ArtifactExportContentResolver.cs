using System.Text.Json;
using Gci409.Application.Common;
using Gci409.Domain.Artifacts;

namespace Gci409.Infrastructure.Exports;

public sealed class ArtifactExportContentResolver : IArtifactExportContentResolver
{
    public string ResolveContent(ArtifactVersion version, OutputFormat format)
    {
        if (version.PrimaryFormat == format)
        {
            return version.Content;
        }

        if (string.IsNullOrWhiteSpace(version.RepresentationsJson))
        {
            throw new ValidationException($"Artifact version cannot be exported as {format}.");
        }

        var representations = JsonSerializer.Deserialize<Dictionary<string, string>>(version.RepresentationsJson)
            ?? throw new ValidationException($"Artifact version cannot be exported as {format}.");

        return representations.TryGetValue(format.ToString(), out var content)
            ? content
            : throw new ValidationException($"Artifact version cannot be exported as {format}.");
    }
}
