using Gci409.Domain.Common;

namespace Gci409.Domain.Requirements;

public enum RequirementType
{
    Functional = 1,
    NonFunctional = 2,
    Integration = 3,
    Security = 4,
    Data = 5,
    Reporting = 6
}

public enum RequirementPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ConstraintType
{
    Business = 1,
    Technical = 2,
    Regulatory = 3,
    Cost = 4,
    Timeline = 5,
    Platform = 6
}

public enum ConstraintSeverity
{
    Advisory = 1,
    Important = 2,
    Mandatory = 3
}

public sealed class RequirementSet : AuditableEntity, IAggregateRoot
{
    private readonly List<RequirementSetVersion> _versions = [];

    private RequirementSet()
    {
    }

    public Guid ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Overview { get; private set; } = string.Empty;

    public int CurrentVersionNumber { get; private set; }

    public IReadOnlyCollection<RequirementSetVersion> Versions => _versions;

    public static RequirementSet Create(Guid projectId, string name, string overview, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new RequirementSet
        {
            ProjectId = projectId,
            Name = name.Trim(),
            Overview = overview.Trim(),
            CurrentVersionNumber = 0,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }

    public void UpdateMetadata(string name, string overview, Guid modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Name = name.Trim();
        Overview = overview.Trim();
        Touch(modifiedByUserId, modifiedAtUtc);
    }

    public RequirementSetVersion AddVersion(
        string summary,
        IEnumerable<RequirementItem> requirements,
        IEnumerable<ConstraintItem> constraints,
        Guid createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        var version = RequirementSetVersion.Create(Id, CurrentVersionNumber + 1, summary, requirements.ToList(), constraints.ToList(), createdByUserId, createdAtUtc);
        _versions.Add(version);
        CurrentVersionNumber = version.VersionNumber;
        Overview = summary.Trim();
        Touch(createdByUserId, createdAtUtc);
        return version;
    }
}

public sealed class RequirementSetVersion : AuditableEntity
{
    private readonly List<RequirementItem> _requirements = [];
    private readonly List<ConstraintItem> _constraints = [];

    private RequirementSetVersion()
    {
    }

    public Guid RequirementSetId { get; private set; }

    public int VersionNumber { get; private set; }

    public string Summary { get; private set; } = string.Empty;

    public RequirementSet RequirementSet { get; private set; } = null!;

    public IReadOnlyCollection<RequirementItem> Requirements => _requirements;

    public IReadOnlyCollection<ConstraintItem> Constraints => _constraints;

    public static RequirementSetVersion Create(
        Guid requirementSetId,
        int versionNumber,
        string summary,
        List<RequirementItem> requirements,
        List<ConstraintItem> constraints,
        Guid createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        var version = new RequirementSetVersion
        {
            RequirementSetId = requirementSetId,
            VersionNumber = versionNumber,
            Summary = summary.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };

        foreach (var requirement in requirements)
        {
            requirement.AttachToVersion(version.Id);
            version._requirements.Add(requirement);
        }

        foreach (var constraint in constraints)
        {
            constraint.AttachToVersion(version.Id);
            version._constraints.Add(constraint);
        }

        return version;
    }
}

public sealed class RequirementItem : Entity
{
    private RequirementItem()
    {
    }

    public Guid RequirementSetVersionId { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public RequirementType Type { get; private set; }

    public RequirementPriority Priority { get; private set; }

    public static RequirementItem Create(string code, string title, string description, RequirementType type, RequirementPriority priority)
    {
        return new RequirementItem
        {
            Code = code.Trim(),
            Title = title.Trim(),
            Description = description.Trim(),
            Type = type,
            Priority = priority
        };
    }

    internal void AttachToVersion(Guid versionId)
    {
        RequirementSetVersionId = versionId;
    }
}

public sealed class ConstraintItem : Entity
{
    private ConstraintItem()
    {
    }

    public Guid RequirementSetVersionId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public ConstraintType Type { get; private set; }

    public ConstraintSeverity Severity { get; private set; }

    public static ConstraintItem Create(string title, string description, ConstraintType type, ConstraintSeverity severity)
    {
        return new ConstraintItem
        {
            Title = title.Trim(),
            Description = description.Trim(),
            Type = type,
            Severity = severity
        };
    }

    internal void AttachToVersion(Guid versionId)
    {
        RequirementSetVersionId = versionId;
    }
}
