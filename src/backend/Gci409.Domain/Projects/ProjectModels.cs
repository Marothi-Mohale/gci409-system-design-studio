using Gci409.Domain.Common;

namespace Gci409.Domain.Projects;

public enum ProjectStatus
{
    Draft = 1,
    Active = 2,
    Archived = 3
}

public enum ProjectRole
{
    Owner = 1,
    Contributor = 2,
    Reviewer = 3,
    Viewer = 4
}

public enum MembershipStatus
{
    Invited = 1,
    Active = 2,
    Removed = 3
}

public sealed class Project : AuditableEntity, IAggregateRoot
{
    private readonly List<ProjectMembership> _memberships = [];

    private Project()
    {
    }

    public string Key { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ProjectStatus Status { get; private set; } = ProjectStatus.Active;

    public Guid OwnerUserId { get; private set; }

    public IReadOnlyCollection<ProjectMembership> Memberships => _memberships;

    public static Project Create(string key, string name, string description, Guid ownerUserId, DateTimeOffset createdAtUtc)
    {
        var project = new Project
        {
            Key = key.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description.Trim(),
            OwnerUserId = ownerUserId,
            Status = ProjectStatus.Active,
            CreatedByUserId = ownerUserId,
            CreatedAtUtc = createdAtUtc
        };

        project._memberships.Add(ProjectMembership.Create(project.Id, ownerUserId, ProjectRole.Owner, createdAtUtc));
        return project;
    }

    public void UpdateDetails(string name, string description, Guid modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Name = name.Trim();
        Description = description.Trim();
        Touch(modifiedByUserId, modifiedAtUtc);
    }
}

public sealed class ProjectMembership : AuditableEntity
{
    private ProjectMembership()
    {
    }

    public Guid ProjectId { get; private set; }

    public Guid UserId { get; private set; }

    public ProjectRole Role { get; private set; }

    public MembershipStatus Status { get; private set; }

    public static ProjectMembership Create(Guid projectId, Guid userId, ProjectRole role, DateTimeOffset createdAtUtc)
    {
        return new ProjectMembership
        {
            ProjectId = projectId,
            UserId = userId,
            Role = role,
            Status = MembershipStatus.Active,
            CreatedByUserId = userId,
            CreatedAtUtc = createdAtUtc
        };
    }
}
