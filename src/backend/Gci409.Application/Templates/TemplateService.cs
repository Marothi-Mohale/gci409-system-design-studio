using Gci409.Application.Common;
using Gci409.Application.Projects;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Projects;
using Gci409.Domain.Templates;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Templates;

public sealed class TemplateService(
    IGci409DbContext dbContext,
    ProjectService projectService,
    IAuditWriter auditWriter,
    IClock clock)
{
    public async Task<IReadOnlyCollection<TemplateSummaryResponse>> ListForProjectAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        return await dbContext.Templates
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.LastModifiedAtUtc ?? x.CreatedAtUtc)
            .Select(x => new TemplateSummaryResponse(x.Id, x.ProjectId, x.Name, x.Description, x.Status, x.CurrentVersionNumber, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<TemplateDetailResponse> GetAsync(Guid templateId, Guid userId, CancellationToken cancellationToken = default)
    {
        var template = await dbContext.Templates
            .Include(x => x.Versions)
            .SingleOrDefaultAsync(x => x.Id == templateId, cancellationToken)
            ?? throw new NotFoundException("Template was not found.");

        if (template.ProjectId.HasValue)
        {
            await projectService.EnsureProjectAccessAsync(template.ProjectId.Value, userId, ProjectRole.Viewer, cancellationToken);
        }

        return MapDetail(template);
    }

    public async Task<TemplateDetailResponse> CreateForProjectAsync(Guid projectId, Guid userId, CreateTemplateRequest request, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Contributor, cancellationToken);

        var template = Template.Create(projectId, request.Name, request.Description, userId, clock.UtcNow);
        template.AddVersion(request.Content, request.ArtifactKinds.Select(x => (int)x).ToList(), userId, clock.UtcNow);

        await dbContext.Templates.AddAsync(template, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, projectId, "template.created", nameof(Template), template.Id.ToString(), $"Created template {template.Name}.", cancellationToken: cancellationToken);

        return MapDetail(template);
    }

    public async Task<TemplateVersionResponse> CreateVersionAsync(Guid templateId, Guid userId, CreateTemplateVersionRequest request, CancellationToken cancellationToken = default)
    {
        var template = await dbContext.Templates
            .Include(x => x.Versions)
            .SingleOrDefaultAsync(x => x.Id == templateId, cancellationToken)
            ?? throw new NotFoundException("Template was not found.");

        if (!template.ProjectId.HasValue)
        {
            throw new ForbiddenException("Global template changes are restricted.");
        }

        await projectService.EnsureProjectAccessAsync(template.ProjectId.Value, userId, ProjectRole.Contributor, cancellationToken);

        var version = template.AddVersion(request.Content, request.ArtifactKinds.Select(x => (int)x).ToList(), userId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, template.ProjectId.Value, "template.version_created", nameof(TemplateVersion), version.Id.ToString(), $"Created template version {version.VersionNumber} for {template.Name}.", cancellationToken: cancellationToken);

        return MapVersion(version);
    }

    private static TemplateDetailResponse MapDetail(Template template)
    {
        return new TemplateDetailResponse(
            template.Id,
            template.ProjectId,
            template.Name,
            template.Description,
            template.Status,
            template.CurrentVersionNumber,
            template.Versions
                .OrderByDescending(x => x.VersionNumber)
                .Select(MapVersion)
                .ToList(),
            template.CreatedAtUtc);
    }

    private static TemplateVersionResponse MapVersion(TemplateVersion version)
    {
        return new TemplateVersionResponse(
            version.Id,
            version.VersionNumber,
            version.Content,
            ParseArtifactKinds(version.SupportedArtifactKindsCsv),
            version.CreatedAtUtc);
    }

    private static IReadOnlyCollection<ArtifactKind> ParseArtifactKinds(string supportedArtifactKindsCsv)
    {
        return supportedArtifactKindsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => (ArtifactKind)int.Parse(x))
            .ToList();
    }
}
