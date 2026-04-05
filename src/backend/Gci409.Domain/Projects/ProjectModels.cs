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

        project._memberships.Add(ProjectMembership.Create(project.Id, ownerUserId, ProjectRole.Owner, ownerUserId, createdAtUtc));
        return project;
    }

    public void UpdateDetails(string name, string description, Guid modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Name = name.Trim();
        Description = description.Trim();
        Touch(modifiedByUserId, modifiedAtUtc);
    }

    public void Archive(Guid modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Status = ProjectStatus.Archived;
        Touch(modifiedByUserId, modifiedAtUtc);
    }

    public ProjectMembership AddMembership(Guid userId, ProjectRole role, Guid addedByUserId, DateTimeOffset createdAtUtc)
    {
        var existingMembership = _memberships.SingleOrDefault(x => x.UserId == userId);
        if (existingMembership is not null)
        {
            existingMembership.SetRole(role, addedByUserId, createdAtUtc);
            return existingMembership;
        }

        var membership = ProjectMembership.Create(Id, userId, role, addedByUserId, createdAtUtc);
        _memberships.Add(membership);
        Touch(addedByUserId, createdAtUtc);
        return membership;
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

    public static ProjectMembership Create(Guid projectId, Guid userId, ProjectRole role, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new ProjectMembership
        {
            ProjectId = projectId,
            UserId = userId,
            Role = role,
            Status = MembershipStatus.Active,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }

    public void SetRole(ProjectRole role, Guid modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Role = role;
        Status = MembershipStatus.Active;
        Touch(modifiedByUserId, modifiedAtUtc);
    }

    public void Remove(Guid modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Status = MembershipStatus.Removed;
        Touch(modifiedByUserId, modifiedAtUtc);
    }
}
