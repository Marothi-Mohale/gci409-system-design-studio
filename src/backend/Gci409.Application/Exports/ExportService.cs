using Gci409.Application.Common;
using Gci409.Application.Projects;
using Gci409.Application.Artifacts;
using Gci409.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Exports;

public sealed class ExportService(IGci409DbContext dbContext, ProjectService projectService)
{
    public async Task<PagedResult<ExportSummaryResponse>> ListAsync(Guid projectId, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        var query =
            from export in dbContext.ArtifactExports.AsNoTracking()
            join version in dbContext.ArtifactVersions.AsNoTracking() on export.ArtifactVersionId equals version.Id
            join artifact in dbContext.GeneratedArtifacts.AsNoTracking() on version.GeneratedArtifactId equals artifact.Id
            where artifact.ProjectId == projectId
            orderby export.CreatedAtUtc descending
            select new ExportSummaryResponse(export.Id, export.ArtifactVersionId, export.Format, export.Status, export.FileName, export.CreatedAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ExportSummaryResponse>(items, page, pageSize, totalCount);
    }

    public async Task<ExportDetailResponse> GetAsync(Guid exportId, Guid userId, CancellationToken cancellationToken = default)
    {
        var export =
            await (
                from exportRecord in dbContext.ArtifactExports.AsNoTracking()
                join version in dbContext.ArtifactVersions.AsNoTracking() on exportRecord.ArtifactVersionId equals version.Id
                join artifact in dbContext.GeneratedArtifacts.AsNoTracking() on version.GeneratedArtifactId equals artifact.Id
                where exportRecord.Id == exportId
                select new
                {
                    exportRecord.Id,
                    exportRecord.ArtifactVersionId,
                    GeneratedArtifactId = artifact.Id,
                    artifact.ProjectId,
                    exportRecord.Format,
                    exportRecord.Status,
                    exportRecord.FileName,
                    exportRecord.Content,
                    exportRecord.CreatedAtUtc
                })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Export was not found.");

        var response = new ExportDetailResponse(
            export.Id,
            export.ArtifactVersionId,
            export.GeneratedArtifactId,
            export.ProjectId,
            export.Format,
            export.Status,
            export.FileName,
            export.Content,
            ArtifactExportFileMetadata.ResolveContentType(export.Format, export.FileName),
            ArtifactExportFileMetadata.ResolveContentEncoding(export.Format),
            export.CreatedAtUtc);

        await projectService.EnsureProjectAccessAsync(response.ProjectId, userId, ProjectRole.Viewer, cancellationToken);
        return response;
    }
}
