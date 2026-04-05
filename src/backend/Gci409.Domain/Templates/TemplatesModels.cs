using Gci409.Domain.Common;

namespace Gci409.Domain.Templates;

public enum TemplateStatus
{
    Draft = 1,
    Active = 2,
    Retired = 3
}

public enum RuleScope
{
    Global = 1,
    Project = 2
}

public sealed class Template : AuditableEntity, IAggregateRoot
{
    private readonly List<TemplateVersion> _versions = [];

    private Template()
    {
    }

    public Guid? ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public TemplateStatus Status { get; private set; }

    public int CurrentVersionNumber { get; private set; }

    public IReadOnlyCollection<TemplateVersion> Versions => _versions;

    public static Template Create(Guid? projectId, string name, string description, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new Template
        {
            ProjectId = projectId,
            Name = name.Trim(),
            Description = description.Trim(),
            Status = TemplateStatus.Active,
            CurrentVersionNumber = 0,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }

    public TemplateVersion AddVersion(string content, IReadOnlyCollection<int> supportedArtifactKinds, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        var version = TemplateVersion.Create(
            Id,
            CurrentVersionNumber + 1,
            content,
            string.Join(",", supportedArtifactKinds.OrderBy(x => x)),
            createdByUserId,
            createdAtUtc);

        _versions.Add(version);
        CurrentVersionNumber = version.VersionNumber;
        Touch(createdByUserId, createdAtUtc);
        return version;
    }

    public void SetStatus(TemplateStatus status, Guid modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Status = status;
        Touch(modifiedByUserId, modifiedAtUtc);
    }
}

public sealed class TemplateVersion : AuditableEntity
{
    private TemplateVersion()
    {
    }

    public Guid TemplateId { get; private set; }

    public int VersionNumber { get; private set; }

    public string Content { get; private set; } = string.Empty;

    public string SupportedArtifactKindsCsv { get; private set; } = string.Empty;

    public static TemplateVersion Create(Guid templateId, int versionNumber, string content, string supportedArtifactKindsCsv, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new TemplateVersion
        {
            TemplateId = templateId,
            VersionNumber = versionNumber,
            Content = content,
            SupportedArtifactKindsCsv = supportedArtifactKindsCsv,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }
}

public sealed class GenerationRule : AuditableEntity, IAggregateRoot
{
    private readonly List<GenerationRuleVersion> _versions = [];

    private GenerationRule()
    {
    }

    public Guid? ProjectId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public RuleScope Scope { get; private set; }

    public int CurrentVersionNumber { get; private set; }

    public IReadOnlyCollection<GenerationRuleVersion> Versions => _versions;

    public static GenerationRule Create(Guid? projectId, string name, string description, RuleScope scope, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new GenerationRule
        {
            ProjectId = projectId,
            Name = name.Trim(),
            Description = description.Trim(),
            Scope = scope,
            CurrentVersionNumber = 0,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }

    public GenerationRuleVersion AddVersion(string ruleDefinitionJson, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        var version = GenerationRuleVersion.Create(Id, CurrentVersionNumber + 1, ruleDefinitionJson, createdByUserId, createdAtUtc);
        _versions.Add(version);
        CurrentVersionNumber = version.VersionNumber;
        Touch(createdByUserId, createdAtUtc);
        return version;
    }
}

public sealed class GenerationRuleVersion : AuditableEntity
{
    private GenerationRuleVersion()
    {
    }

    public Guid GenerationRuleId { get; private set; }

    public int VersionNumber { get; private set; }

    public string RuleDefinitionJson { get; private set; } = string.Empty;

    public static GenerationRuleVersion Create(Guid generationRuleId, int versionNumber, string ruleDefinitionJson, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new GenerationRuleVersion
        {
            GenerationRuleId = generationRuleId,
            VersionNumber = versionNumber,
            RuleDefinitionJson = ruleDefinitionJson,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }
}
