using Gci409.Application.Common;
using Gci409.Application.Projects;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Artifacts;

public sealed class ArtifactService(
    IGci409DbContext dbContext,
    ProjectService projectService,
    IArtifactExportContentResolver artifactExportContentResolver,
    IArtifactPdfRenderer artifactPdfRenderer,
    IAuditWriter auditWriter,
    IClock clock)
{
    public async Task<IReadOnlyCollection<ArtifactSummaryResponse>> ListAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        return await dbContext.GeneratedArtifacts
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.LastModifiedAtUtc ?? x.CreatedAtUtc)
            .Select(x => new ArtifactSummaryResponse(x.Id, x.ArtifactKind, x.Title, x.Status, x.CurrentVersionNumber, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<ArtifactVersionResponse>> GetVersionsAsync(Guid projectId, Guid artifactId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        return await dbContext.ArtifactVersions
            .AsNoTracking()
            .Where(x => x.GeneratedArtifactId == artifactId && x.GeneratedArtifact.ProjectId == projectId)
            .OrderByDescending(x => x.VersionNumber)
            .Select(x => new ArtifactVersionResponse(x.Id, x.VersionNumber, x.PrimaryFormat, x.Summary, x.Content, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<ExportResponse> ExportAsync(Guid artifactVersionId, Guid userId, CreateExportRequest request, CancellationToken cancellationToken = default)
    {
        var version = await dbContext.ArtifactVersions
            .Include(x => x.GeneratedArtifact)
            .SingleOrDefaultAsync(x => x.Id == artifactVersionId, cancellationToken)
            ?? throw new NotFoundException("Artifact version not found.");

        await projectService.EnsureProjectAccessAsync(version.GeneratedArtifact.ProjectId, userId, ProjectRole.Viewer, cancellationToken);

        var sourceContent = artifactExportContentResolver.ResolveContent(version, ResolveSourceFormat(version, request.Format));
        var content = request.Format == OutputFormat.Pdf
            ? Convert.ToBase64String(
                artifactPdfRenderer.Render(
                    new ArtifactPdfRenderRequest(
                        version.GeneratedArtifact.Title,
                        version.GeneratedArtifact.ArtifactKind,
                        version.VersionNumber,
                        version.Summary,
                        ResolveSourceFormat(version, request.Format),
                        sourceContent,
                        clock.UtcNow)))
            : sourceContent;
        var fileName = $"{version.GeneratedArtifact.Title.Replace(' ', '-')}-v{version.VersionNumber}.{ResolveExtension(request.Format)}";
        var export = version.AddExport(request.Format, fileName, content, userId, clock.UtcNow);
        await dbContext.ArtifactExports.AddAsync(export, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, version.GeneratedArtifact.ProjectId, "artifact.exported", nameof(ArtifactExport), export.Id.ToString(), $"Exported artifact version {version.VersionNumber} as {request.Format}.", cancellationToken: cancellationToken);

        return new ExportResponse(
            export.Id,
            export.Format,
            export.FileName,
            ArtifactExportFileMetadata.IsBinary(export.Format) ? null : export.Content,
            ArtifactExportFileMetadata.ResolveContentType(export.Format, export.FileName),
            ArtifactExportFileMetadata.ResolveContentEncoding(export.Format),
            $"/api/exports/{export.Id}/download",
            export.CreatedAtUtc);
    }

    private static OutputFormat ResolveSourceFormat(ArtifactVersion version, OutputFormat requestedFormat)
    {
        if (requestedFormat != OutputFormat.Pdf)
        {
            return requestedFormat;
        }

        return version.PrimaryFormat;
    }

    private static string ResolveExtension(OutputFormat format)
    {
        return format switch
        {
            OutputFormat.Markdown => "md",
            OutputFormat.Mermaid => "mmd",
            OutputFormat.PlantUml => "puml",
            OutputFormat.Pdf => "pdf",
            OutputFormat.Png => "png",
            _ => "txt"
        };
    }
}
