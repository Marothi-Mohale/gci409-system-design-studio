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
