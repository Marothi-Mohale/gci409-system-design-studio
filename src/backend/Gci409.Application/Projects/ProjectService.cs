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
