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
}

public sealed class GenerationRuleVersion : AuditableEntity
{
    private GenerationRuleVersion()
    {
    }

    public Guid GenerationRuleId { get; private set; }

    public int VersionNumber { get; private set; }

    public string RuleDefinitionJson { get; private set; } = string.Empty;
}
