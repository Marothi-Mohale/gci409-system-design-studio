using System.Globalization;
using System.Text;
using Gci409.Application.Common;
using Gci409.Domain.Artifacts;

namespace Gci409.Infrastructure.Exports;

public sealed class ArtifactPdfRenderer : IArtifactPdfRenderer
{
    private const float PageWidth = 595f;
    private const float PageHeight = 842f;
    private const float LeftMargin = 48f;
    private const float TopMargin = 56f;
    private const float BottomMargin = 52f;
    private const float RegularFontSize = 10f;
    private const float HeadingFontSize = 12f;
    private const float TitleFontSize = 18f;
    private const float LineHeight = 14f;
    private const int WrapWidth = 92;
    private const int HeadingWrapWidth = 72;

    public byte[] Render(ArtifactPdfRenderRequest request)
    {
        var pages = Paginate(BuildDocumentLines(request));
        return BuildPdfDocument(request.Title, pages);
    }

    private static List<PdfLine> BuildDocumentLines(ArtifactPdfRenderRequest request)
    {
        var lines = new List<PdfLine>
        {
            new(request.Title, PdfLineStyle.Title),
            new($"Artifact Type: {ToLabel(request.ArtifactKind)}", PdfLineStyle.Regular),
            new($"Version: v{request.VersionNumber}", PdfLineStyle.Regular),
            new($"Exported: {request.CreatedAtUtc.ToString("u", CultureInfo.InvariantCulture)}", PdfLineStyle.Regular),
            new($"Source Format: {ToLabel(request.SourceFormat)}", PdfLineStyle.Regular),
            PdfLine.Blank,
            new("Summary", PdfLineStyle.Heading)
        };

        lines.AddRange(WrapBlock(request.Summary, PdfLineStyle.Regular));
        lines.Add(PdfLine.Blank);
        lines.Add(new("Artifact Content", PdfLineStyle.Heading));
        lines.AddRange(WrapPreservingParagraphs(request.SourceContent, PdfLineStyle.Monospace));

        return lines;
    }

    private static List<List<PdfLine>> Paginate(IReadOnlyCollection<PdfLine> lines)
    {
        var pages = new List<List<PdfLine>>();
        var currentPage = new List<PdfLine>();
        var availableHeight = PageHeight - TopMargin - BottomMargin;
        var usedHeight = 0f;

        foreach (var line in lines)
        {
            var nextHeight = GetLineHeight(line.Style);
            if (currentPage.Count > 0 && usedHeight + nextHeight > availableHeight)
            {
                pages.Add(currentPage);
                currentPage = new List<PdfLine>();
                usedHeight = 0f;
            }

            currentPage.Add(line);
            usedHeight += nextHeight;
        }

        if (currentPage.Count > 0)
        {
            pages.Add(currentPage);
        }

        if (pages.Count == 0)
        {
            pages.Add(new List<PdfLine> { new PdfLine("No content available.", PdfLineStyle.Regular) });
        }

        return pages;
    }

    private static byte[] BuildPdfDocument(string title, IReadOnlyList<List<PdfLine>> pages)
    {
        var objects = new List<byte[]>();
        const int catalogObjectNumber = 1;
        const int pagesObjectNumber = 2;
        const int regularFontObjectNumber = 3;
        const int boldFontObjectNumber = 4;
        const int monoFontObjectNumber = 5;
        var nextObjectNumber = 6;
        var pageObjectNumbers = new List<int>();

        objects.Add(Encoding.ASCII.GetBytes("1 0 obj\n<< /Type /Catalog /Pages 2 0 R >>\nendobj\n"));
        objects.Add(Array.Empty<byte>());
        objects.Add(Encoding.ASCII.GetBytes("3 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\nendobj\n"));
        objects.Add(Encoding.ASCII.GetBytes("4 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>\nendobj\n"));
        objects.Add(Encoding.ASCII.GetBytes("5 0 obj\n<< /Type /Font /Subtype /Type1 /BaseFont /Courier >>\nendobj\n"));

        foreach (var page in pages)
        {
            var pageObjectNumber = nextObjectNumber++;
            var contentObjectNumber = nextObjectNumber++;
            pageObjectNumbers.Add(pageObjectNumber);

            var contentStream = BuildPageContentStream(page, title);
            var contentObject = BuildStreamObject(contentObjectNumber, contentStream);
            var pageObject = Encoding.ASCII.GetBytes($"""
{pageObjectNumber} 0 obj
<< /Type /Page /Parent {pagesObjectNumber} 0 R /MediaBox [0 0 {Format(PageWidth)} {Format(PageHeight)}] /Resources << /Font << /F1 {regularFontObjectNumber} 0 R /F2 {boldFontObjectNumber} 0 R /F3 {monoFontObjectNumber} 0 R >> >> /Contents {contentObjectNumber} 0 R >>
endobj
""");

            objects.Add(pageObject);
            objects.Add(contentObject);
        }

        var kids = string.Join(' ', pageObjectNumbers.Select(x => $"{x} 0 R"));
        objects[pagesObjectNumber - 1] = Encoding.ASCII.GetBytes($"""
{pagesObjectNumber} 0 obj
<< /Type /Pages /Count {pageObjectNumbers.Count} /Kids [{kids}] >>
endobj
""");

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34, 0x0A, 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A });

        var xrefOffsets = new List<long> { 0L };
        foreach (var obj in objects)
        {
            xrefOffsets.Add(stream.Position);
            writer.Write(obj);
        }

        var xrefStart = stream.Position;
        writer.Write(Encoding.ASCII.GetBytes($"xref\n0 {objects.Count + 1}\n"));
        writer.Write(Encoding.ASCII.GetBytes("0000000000 65535 f \n"));
        foreach (var offset in xrefOffsets.Skip(1))
        {
            writer.Write(Encoding.ASCII.GetBytes($"{offset:D10} 00000 n \n"));
        }

        writer.Write(Encoding.ASCII.GetBytes($"""
trailer
<< /Size {objects.Count + 1} /Root {catalogObjectNumber} 0 R >>
startxref
{xrefStart}
%%EOF
"""));

        writer.Flush();
        return stream.ToArray();
    }

    private static byte[] BuildStreamObject(int objectNumber, string contentStream)
    {
        var contentBytes = Encoding.ASCII.GetBytes(contentStream);
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.ASCII, leaveOpen: true);
        writer.Write(Encoding.ASCII.GetBytes($"{objectNumber} 0 obj\n<< /Length {contentBytes.Length} >>\nstream\n"));
        writer.Write(contentBytes);
        writer.Write(Encoding.ASCII.GetBytes("\nendstream\nendobj\n"));
        writer.Flush();
        return stream.ToArray();
    }

    private static string BuildPageContentStream(IReadOnlyCollection<PdfLine> lines, string title)
    {
        var builder = new StringBuilder();
        builder.AppendLine("BT");
        builder.AppendLine($"/F2 {Format(TitleFontSize)} Tf");
        builder.AppendLine($"{Format(LeftMargin)} {Format(PageHeight - TopMargin)} Td");
        builder.AppendLine($"({EscapePdfText(title)}) Tj");

        var remainingLines = lines.ToList();
        if (remainingLines.Count > 0 && remainingLines[0].Text == title)
        {
            remainingLines.RemoveAt(0);
        }

        foreach (var line in remainingLines)
        {
            builder.AppendLine($"0 -{Format(GetLineHeight(line.Style))} Td");
            if (string.IsNullOrWhiteSpace(line.Text))
            {
                continue;
            }

            builder.AppendLine($"/{ResolveFont(line.Style)} {Format(ResolveFontSize(line.Style))} Tf");
            builder.AppendLine($"({EscapePdfText(line.Text)}) Tj");
        }

        builder.AppendLine("ET");
        return builder.ToString();
    }

    private static IReadOnlyCollection<PdfLine> WrapPreservingParagraphs(string content, PdfLineStyle style)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [new PdfLine("No source content available.", style)];
        }

        var lines = new List<PdfLine>();
        var paragraphs = content.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        foreach (var paragraph in paragraphs)
        {
            if (string.IsNullOrWhiteSpace(paragraph))
            {
                lines.Add(PdfLine.Blank);
                continue;
            }

            lines.AddRange(WrapBlock(paragraph, style));
        }

        return lines;
    }

    private static IReadOnlyCollection<PdfLine> WrapBlock(string text, PdfLineStyle style)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [PdfLine.Blank];
        }

        var maxWidth = style == PdfLineStyle.Heading ? HeadingWrapWidth : WrapWidth;
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var lines = new List<PdfLine>();
        var current = new StringBuilder();

        foreach (var word in words)
        {
            if (current.Length == 0)
            {
                current.Append(word);
                continue;
            }

            if (current.Length + 1 + word.Length <= maxWidth)
            {
                current.Append(' ').Append(word);
                continue;
            }

            lines.Add(new PdfLine(current.ToString(), style));
            current.Clear();
            current.Append(word);
        }

        if (current.Length > 0)
        {
            lines.Add(new PdfLine(current.ToString(), style));
        }

        return lines;
    }

    private static float GetLineHeight(PdfLineStyle style)
    {
        return style == PdfLineStyle.Title ? 22f : LineHeight;
    }

    private static float ResolveFontSize(PdfLineStyle style)
    {
        return style switch
        {
            PdfLineStyle.Title => TitleFontSize,
            PdfLineStyle.Heading => HeadingFontSize,
            PdfLineStyle.Monospace => RegularFontSize,
            _ => RegularFontSize
        };
    }

    private static string ResolveFont(PdfLineStyle style)
    {
        return style switch
        {
            PdfLineStyle.Title => "F2",
            PdfLineStyle.Heading => "F2",
            PdfLineStyle.Monospace => "F3",
            _ => "F1"
        };
    }

    private static string EscapePdfText(string value)
    {
        var sanitized = value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);

        return new string(sanitized.Select(ch => ch is >= ' ' and <= '~' ? ch : '?').ToArray());
    }

    private static string ToLabel(ArtifactKind artifactKind)
    {
        return artifactKind switch
        {
            ArtifactKind.UseCaseDiagram => "Use Case Diagram",
            ArtifactKind.ClassDiagram => "Class Diagram",
            ArtifactKind.SequenceDiagram => "Sequence Diagram",
            ArtifactKind.ActivityDiagram => "Activity Diagram",
            ArtifactKind.ComponentDiagram => "Component Diagram",
            ArtifactKind.DeploymentDiagram => "Deployment Diagram",
            ArtifactKind.ContextDiagram => "Context Diagram",
            ArtifactKind.DataFlowDiagram => "Data Flow Diagram",
            ArtifactKind.Erd => "Entity Relationship Diagram",
            ArtifactKind.ArchitectureSummary => "Architecture Summary",
            ArtifactKind.ModuleDecomposition => "Module Decomposition",
            ArtifactKind.ApiDesignSuggestion => "API Design Suggestion",
            ArtifactKind.DatabaseDesignSuggestion => "Database Design Suggestion",
            _ => artifactKind.ToString()
        };
    }

    private static string ToLabel(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Markdown => "Markdown",
            OutputFormat.Mermaid => "Mermaid",
            OutputFormat.PlantUml => "PlantUML",
            OutputFormat.Pdf => "PDF",
            OutputFormat.Png => "PNG",
            _ => format.ToString()
        };
    }

    private static string Format(float value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private sealed record PdfLine(string Text, PdfLineStyle Style)
    {
        public static PdfLine Blank { get; } = new(string.Empty, PdfLineStyle.Regular);
    }

    private enum PdfLineStyle
    {
        Title,
        Heading,
        Regular,
        Monospace
    }
}
