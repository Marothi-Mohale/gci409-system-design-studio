using FluentValidation;
using Gci409.Domain.Artifacts;

namespace Gci409.Application.Artifacts;

public sealed record ArtifactSummaryResponse(Guid Id, ArtifactKind ArtifactKind, string Title, ArtifactStatus Status, int CurrentVersionNumber, DateTimeOffset CreatedAtUtc);

public sealed record ArtifactVersionResponse(Guid Id, int VersionNumber, OutputFormat PrimaryFormat, string Summary, string Content, DateTimeOffset CreatedAtUtc);

public sealed record CreateExportRequest(OutputFormat Format);

public sealed record ExportResponse(
    Guid Id,
    OutputFormat Format,
    string FileName,
    string? Content,
    string ContentType,
    string? ContentEncoding,
    string DownloadUrl,
    DateTimeOffset CreatedAtUtc);

public sealed class CreateExportRequestValidator : AbstractValidator<CreateExportRequest>
{
    public CreateExportRequestValidator()
    {
        RuleFor(x => x.Format).IsInEnum();
    }
}

public static class ArtifactExportFileMetadata
{
    public static bool IsBinary(OutputFormat format)
    {
        return format == OutputFormat.Pdf || format == OutputFormat.Png;
    }

    public static string ResolveContentEncoding(OutputFormat format)
    {
        return IsBinary(format) ? "base64" : "utf-8";
    }

    public static string ResolveContentType(OutputFormat format, string fileName)
    {
        return format switch
        {
            OutputFormat.Pdf => "application/pdf",
            OutputFormat.Markdown => "text/markdown",
            OutputFormat.Mermaid => "text/plain",
            OutputFormat.PlantUml => "text/plain",
            OutputFormat.Png => "image/png",
            _ => "application/octet-stream"
        };
    }
}
