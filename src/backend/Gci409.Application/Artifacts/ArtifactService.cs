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
            .Include(x => x.Exports)
            .SingleOrDefaultAsync(x => x.Id == artifactVersionId, cancellationToken)
            ?? throw new NotFoundException("Artifact version not found.");

        await projectService.EnsureProjectAccessAsync(version.GeneratedArtifact.ProjectId, userId, ProjectRole.Viewer, cancellationToken);

        var content = artifactExportContentResolver.ResolveContent(version, request.Format);
        var fileName = $"{version.GeneratedArtifact.Title.Replace(' ', '-')}-v{version.VersionNumber}.{ResolveExtension(request.Format)}";
        var export = version.AddExport(request.Format, fileName, content, userId, clock.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, version.GeneratedArtifact.ProjectId, "artifact.exported", nameof(ArtifactExport), export.Id.ToString(), $"Exported artifact version {version.VersionNumber} as {request.Format}.", cancellationToken: cancellationToken);

        return new ExportResponse(export.Id, export.Format, export.FileName, export.Content, export.CreatedAtUtc);
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
