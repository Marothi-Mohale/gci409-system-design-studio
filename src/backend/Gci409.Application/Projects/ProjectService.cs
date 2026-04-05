using Gci409.Application.Common;
using Gci409.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Projects;

public sealed class ProjectService(IGci409DbContext dbContext, IAuditWriter auditWriter, IClock clock)
{
    public async Task<IReadOnlyCollection<ProjectSummary>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ProjectMemberships
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Status == MembershipStatus.Active)
            .Join(
                dbContext.Projects,
                membership => membership.ProjectId,
                project => project.Id,
                (membership, project) => new ProjectSummary(project.Id, project.Key, project.Name, project.Description, project.Status, membership.Role, project.CreatedAtUtc))
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectDetail> GetAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        var project = await dbContext.Projects
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        var members = await dbContext.ProjectMemberships
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId)
            .Select(x => new ProjectMemberSummary(x.UserId, x.Role, x.Status))
            .ToListAsync(cancellationToken);

        return new ProjectDetail(project.Id, project.Key, project.Name, project.Description, project.Status, members);
    }

    public async Task<ProjectSummary> CreateAsync(Guid userId, CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var key = await GenerateProjectKeyAsync(request.Name, cancellationToken);
        var project = Project.Create(key, request.Name, request.Description, userId, clock.UtcNow);

        await dbContext.Projects.AddAsync(project, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, project.Id, "project.created", nameof(Project), project.Id.ToString(), $"Created project {project.Name}.", cancellationToken: cancellationToken);

        return new ProjectSummary(project.Id, project.Key, project.Name, project.Description, project.Status, ProjectRole.Owner, project.CreatedAtUtc);
    }

    public async Task<ProjectDetail> UpdateAsync(Guid projectId, Guid userId, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, userId, ProjectRole.Owner, cancellationToken);

        var project = await dbContext.Projects.SingleOrDefaultAsync(x => x.Id == projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        project.UpdateDetails(request.Name, request.Description, userId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, projectId, "project.updated", nameof(Project), project.Id.ToString(), $"Updated project {project.Name}.", cancellationToken: cancellationToken);

        return await GetAsync(projectId, userId, cancellationToken);
    }

    public async Task ArchiveAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, userId, ProjectRole.Owner, cancellationToken);

        var project = await dbContext.Projects.SingleOrDefaultAsync(x => x.Id == projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        project.Archive(userId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, projectId, "project.archived", nameof(Project), project.Id.ToString(), $"Archived project {project.Name}.", cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProjectMemberSummary>> GetCollaboratorsAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        return await dbContext.ProjectMemberships
            .AsNoTracking()
            .Where(x => x.ProjectId == projectId && x.Status == MembershipStatus.Active)
            .OrderBy(x => x.Role)
            .ThenBy(x => x.CreatedAtUtc)
            .Select(x => new ProjectMemberSummary(x.UserId, x.Role, x.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<ProjectMemberSummary> AddCollaboratorAsync(Guid projectId, Guid actorUserId, AddCollaboratorRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, actorUserId, ProjectRole.Owner, cancellationToken);

        var project = await dbContext.Projects
            .Include(x => x.Memberships)
            .SingleOrDefaultAsync(x => x.Id == projectId, cancellationToken)
            ?? throw new NotFoundException("Project was not found.");

        var targetUserExists = await dbContext.Users.AnyAsync(x => x.Id == request.UserId, cancellationToken);
        if (!targetUserExists)
        {
            throw new NotFoundException("The collaborator user was not found.");
        }

        var hasExistingMembership = project.Memberships.Any(x => x.UserId == request.UserId);
        var membership = project.AddMembership(request.UserId, request.Role, actorUserId, clock.UtcNow);
        if (!hasExistingMembership)
        {
            await dbContext.ProjectMemberships.AddAsync(membership, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(actorUserId, projectId, "project.collaborator_added", nameof(ProjectMembership), membership.Id.ToString(), $"Added collaborator {request.UserId} as {request.Role}.", cancellationToken: cancellationToken);

        return new ProjectMemberSummary(membership.UserId, membership.Role, membership.Status);
    }

    public async Task<ProjectMemberSummary> UpdateCollaboratorAsync(Guid projectId, Guid membershipUserId, Guid actorUserId, UpdateCollaboratorRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, actorUserId, ProjectRole.Owner, cancellationToken);

        var membership = await dbContext.ProjectMemberships
            .SingleOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == membershipUserId, cancellationToken)
            ?? throw new NotFoundException("Collaborator membership was not found.");

        if (membership.Role == ProjectRole.Owner)
        {
            throw new ValidationException("Project ownership cannot be changed through this endpoint.");
        }

        membership.SetRole(request.Role, actorUserId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(actorUserId, projectId, "project.collaborator_updated", nameof(ProjectMembership), membership.Id.ToString(), $"Updated collaborator {membership.UserId} to role {request.Role}.", cancellationToken: cancellationToken);

        return new ProjectMemberSummary(membership.UserId, membership.Role, membership.Status);
    }

    public async Task RemoveCollaboratorAsync(Guid projectId, Guid membershipUserId, Guid actorUserId, CancellationToken cancellationToken = default)
    {
        await EnsureProjectAccessAsync(projectId, actorUserId, ProjectRole.Owner, cancellationToken);

        var membership = await dbContext.ProjectMemberships
            .SingleOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == membershipUserId, cancellationToken)
            ?? throw new NotFoundException("Collaborator membership was not found.");

        if (membership.Role == ProjectRole.Owner)
        {
            throw new ValidationException("Project owners cannot be removed.");
        }

        membership.Remove(actorUserId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(actorUserId, projectId, "project.collaborator_removed", nameof(ProjectMembership), membership.Id.ToString(), $"Removed collaborator {membership.UserId} from the project.", cancellationToken: cancellationToken);
    }

    public async Task EnsureProjectAccessAsync(Guid projectId, Guid userId, ProjectRole minimumRole, CancellationToken cancellationToken = default)
    {
        var membership = await dbContext.ProjectMemberships
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.ProjectId == projectId && x.UserId == userId && x.Status == MembershipStatus.Active, cancellationToken);

        if (membership is null || membership.Role > minimumRole)
        {
            throw new ForbiddenException("You do not have access to this project.");
        }
    }

    private async Task<string> GenerateProjectKeyAsync(string name, CancellationToken cancellationToken)
    {
        var seed = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
        seed = string.IsNullOrWhiteSpace(seed) ? "GCI409" : seed[..Math.Min(seed.Length, 8)];

        var candidate = seed;
        var counter = 1;

        while (await dbContext.Projects.AnyAsync(x => x.Key == candidate, cancellationToken))
        {
            candidate = $"{seed[..Math.Min(seed.Length, 6)]}{counter:00}";
            counter++;
        }

        return candidate;
    }
}
