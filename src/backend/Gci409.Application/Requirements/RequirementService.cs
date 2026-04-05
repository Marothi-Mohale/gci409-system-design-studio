using Gci409.Application.Common;
using Gci409.Application.Projects;
using Gci409.Domain.Projects;
using Gci409.Domain.Requirements;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Requirements;

public sealed class RequirementService(
    IGci409DbContext dbContext,
    ProjectService projectService,
    IRequirementBaselineBootstrapper requirementBaselineBootstrapper,
    IAuditWriter auditWriter,
    IClock clock)
{
    public async Task<RequirementSetVersionResponse> SaveCurrentAsync(Guid projectId, Guid userId, SaveRequirementSetRequest request, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Contributor, cancellationToken);

        var requirementSet = await dbContext.RequirementSets
            .Include(x => x.Versions)
            .ThenInclude(x => x.Requirements)
            .Include(x => x.Versions)
            .ThenInclude(x => x.Constraints)
            .SingleOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);

        var isNew = requirementSet is null;
        requirementSet ??= RequirementSet.Create(projectId, request.Name, request.Summary, userId, clock.UtcNow);

        if (isNew)
        {
            await dbContext.RequirementSets.AddAsync(requirementSet, cancellationToken);
        }
        else
        {
            requirementSet.UpdateMetadata(request.Name, request.Summary, userId, clock.UtcNow);
        }

        var requirements = request.Requirements
            .Select(x => RequirementItem.Create(x.Code, x.Title, x.Description, x.Type, x.Priority))
            .ToList();
        var constraints = request.Constraints
            .Select(x => ConstraintItem.Create(x.Title, x.Description, x.Type, x.Severity))
            .ToList();

        var version = requirementSet.AddVersion(request.Summary, requirements, constraints, userId, clock.UtcNow);
        if (!isNew)
        {
            await dbContext.RequirementSetVersions.AddAsync(version, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, projectId, "requirements.saved", nameof(RequirementSetVersion), version.Id.ToString(), $"Saved requirement set version {version.VersionNumber}.", cancellationToken: cancellationToken);

        return new RequirementSetVersionResponse(
            requirementSet.Id,
            version.Id,
            requirementSet.Name,
            version.VersionNumber,
            version.Summary,
            version.Requirements.Select(x => new RequirementInput(x.Code, x.Title, x.Description, x.Type, x.Priority)).ToList(),
            version.Constraints.Select(x => new ConstraintInput(x.Title, x.Description, x.Type, x.Severity)).ToList(),
            version.CreatedAtUtc);
    }

    public async Task<RequirementSetVersionResponse?> GetCurrentAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        var requirementSet = await dbContext.RequirementSets
            .AsNoTracking()
            .Include(x => x.Versions)
            .ThenInclude(x => x.Requirements)
            .Include(x => x.Versions)
            .ThenInclude(x => x.Constraints)
            .SingleOrDefaultAsync(x => x.ProjectId == projectId, cancellationToken);

        var version = requirementSet?.Versions.OrderByDescending(x => x.VersionNumber).FirstOrDefault();
        if (requirementSet is null || version is null)
        {
            return null;
        }

        return new RequirementSetVersionResponse(
            requirementSet.Id,
            version.Id,
            requirementSet.Name,
            version.VersionNumber,
            version.Summary,
            version.Requirements.Select(x => new RequirementInput(x.Code, x.Title, x.Description, x.Type, x.Priority)).ToList(),
            version.Constraints.Select(x => new ConstraintInput(x.Title, x.Description, x.Type, x.Severity)).ToList(),
            version.CreatedAtUtc);
    }

    public async Task<RequirementSetVersionResponse> BootstrapFromProjectBriefAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Contributor, cancellationToken);

        var current = await GetCurrentAsync(projectId, userId, cancellationToken);
        if (current is not null)
        {
            return current;
        }

        var project = await dbContext.Projects
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        var draft = requirementBaselineBootstrapper.BuildFromProjectBrief(project.Name, project.Description);
        return await SaveCurrentAsync(projectId, userId, new SaveRequirementSetRequest(draft.Name, draft.Summary, draft.Requirements, draft.Constraints), cancellationToken);
    }
}
