using System.Text;
using Gci409.Application.Artifacts;
using Gci409.Application.Common;
using Gci409.Domain.Artifacts;
using Gci409.Infrastructure.Exports;

namespace Gci409.ApplicationTests;

public sealed class PdfExportTests
{
    private readonly ArtifactPdfRenderer _renderer = new();

    [Fact]
    public void ArtifactPdfRenderer_ShouldGenerateShareablePdfBytes()
    {
        var bytes = _renderer.Render(
            new ArtifactPdfRenderRequest(
                "LoanFlow Architecture Summary",
                ArtifactKind.ArchitectureSummary,
                1,
                "Architecture baseline for LoanFlow Pro.",
                OutputFormat.Markdown,
                "# Architecture Summary\n\n- React frontend\n- ASP.NET Core backend\n- PostgreSQL persistence",
                DateTimeOffset.UtcNow));

        var prefix = Encoding.ASCII.GetString(bytes.Take(4).ToArray());

        Assert.Equal("%PDF", prefix);
        Assert.True(bytes.Length > 500);
    }

    [Fact]
    public void ArtifactExportFileMetadata_ShouldDescribePdfAsBinaryDownload()
    {
        Assert.True(ArtifactExportFileMetadata.IsBinary(OutputFormat.Pdf));
        Assert.Equal("base64", ArtifactExportFileMetadata.ResolveContentEncoding(OutputFormat.Pdf));
        Assert.Equal("application/pdf", ArtifactExportFileMetadata.ResolveContentType(OutputFormat.Pdf, "artifact.pdf"));
    }
}
