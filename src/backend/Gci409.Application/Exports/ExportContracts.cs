using Gci409.Domain.Artifacts;

namespace Gci409.Application.Exports;

public sealed record ExportSummaryResponse(Guid Id, Guid ArtifactVersionId, OutputFormat Format, ExportStatus Status, string FileName, DateTimeOffset CreatedAtUtc);

public sealed record ExportDetailResponse(
    Guid Id,
    Guid ArtifactVersionId,
    Guid GeneratedArtifactId,
    Guid ProjectId,
    OutputFormat Format,
    ExportStatus Status,
    string FileName,
    string Content,
    string ContentType,
    string? ContentEncoding,
    DateTimeOffset CreatedAtUtc);
