using Gci409.Domain.Common;

namespace Gci409.Domain.Artifacts;

public enum ArtifactKind
{
    UseCaseDiagram = 1,
    ClassDiagram = 2,
    SequenceDiagram = 3,
    ActivityDiagram = 4,
    ComponentDiagram = 5,
    DeploymentDiagram = 6,
    ContextDiagram = 7,
    DataFlowDiagram = 8,
    Erd = 9,
    ArchitectureSummary = 10,
    ModuleDecomposition = 11,
    ApiDesignSuggestion = 12,
    DatabaseDesignSuggestion = 13
}

public enum UmlDiagramType
{
    None = 0,
    UseCase = 1,
    Class = 2,
    Sequence = 3,
    Activity = 4,
    Component = 5,
    Deployment = 6
}

public enum OutputFormat
{
    Markdown = 1,
    Mermaid = 2,
    PlantUml = 3,
    Pdf = 4,
    Png = 5
}

public enum ArtifactStatus
{
    Draft = 1,
    Reviewed = 2,
    Approved = 3,
    Superseded = 4
}

public enum ExportStatus
{
    Queued = 1,
    Completed = 2,
    Failed = 3
}

public sealed class GeneratedArtifact : AuditableEntity, IAggregateRoot
{
    private readonly List<ArtifactVersion> _versions = [];

    private GeneratedArtifact()
    {
    }

    public Guid ProjectId { get; private set; }

    public ArtifactKind ArtifactKind { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public ArtifactStatus Status { get; private set; } = ArtifactStatus.Draft;

    public int CurrentVersionNumber { get; private set; }

    public UmlArtifactProfile? UmlProfile { get; private set; }

    public IReadOnlyCollection<ArtifactVersion> Versions => _versions;

    public static GeneratedArtifact Create(Guid projectId, ArtifactKind artifactKind, string title, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new GeneratedArtifact
        {
            ProjectId = projectId,
            ArtifactKind = artifactKind,
            Title = title.Trim(),
            Status = ArtifactStatus.Draft,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }

    public ArtifactVersion AddVersion(
        OutputFormat primaryFormat,
        string summary,
        string content,
        string? representationsJson,
        Guid? generationRequestId,
        Guid createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        var version = ArtifactVersion.Create(Id, CurrentVersionNumber + 1, primaryFormat, summary, content, representationsJson, generationRequestId, createdByUserId, createdAtUtc);
        _versions.Add(version);
        CurrentVersionNumber = version.VersionNumber;
        Touch(createdByUserId, createdAtUtc);
        return version;
    }

    public void EnsureUmlProfile(UmlDiagramType diagramType)
    {
        UmlProfile ??= UmlArtifactProfile.Create(Id, diagramType);
    }

    public void SetStatus(ArtifactStatus status, Guid modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Status = status;
        Touch(modifiedByUserId, modifiedAtUtc);
    }
}

public sealed class UmlArtifactProfile : Entity
{
    private UmlArtifactProfile()
    {
    }

    public Guid GeneratedArtifactId { get; private set; }

    public UmlDiagramType DiagramType { get; private set; }

    public bool SupportsMermaid { get; private set; } = true;

    public bool SupportsPlantUml { get; private set; } = true;

    public static UmlArtifactProfile Create(Guid generatedArtifactId, UmlDiagramType diagramType)
    {
        return new UmlArtifactProfile
        {
            GeneratedArtifactId = generatedArtifactId,
            DiagramType = diagramType
        };
    }
}

public sealed class ArtifactVersion : AuditableEntity
{
    private readonly List<ArtifactExport> _exports = [];

    private ArtifactVersion()
    {
    }

    public Guid GeneratedArtifactId { get; private set; }

    public GeneratedArtifact GeneratedArtifact { get; private set; } = null!;

    public int VersionNumber { get; private set; }

    public Guid? GenerationRequestId { get; private set; }

    public OutputFormat PrimaryFormat { get; private set; }

    public string Summary { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public string? RepresentationsJson { get; private set; }

    public IReadOnlyCollection<ArtifactExport> Exports => _exports;

    public static ArtifactVersion Create(
        Guid generatedArtifactId,
        int versionNumber,
        OutputFormat primaryFormat,
        string summary,
        string content,
        string? representationsJson,
        Guid? generationRequestId,
        Guid createdByUserId,
        DateTimeOffset createdAtUtc)
    {
        return new ArtifactVersion
        {
            GeneratedArtifactId = generatedArtifactId,
            VersionNumber = versionNumber,
            PrimaryFormat = primaryFormat,
            Summary = summary.Trim(),
            Content = content,
            RepresentationsJson = representationsJson,
            GenerationRequestId = generationRequestId,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }

    public ArtifactExport AddExport(OutputFormat format, string fileName, string content, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        var export = ArtifactExport.Create(Id, format, fileName, content, createdByUserId, createdAtUtc);
        _exports.Add(export);
        return export;
    }
}

public sealed class ArtifactExport : AuditableEntity
{
    private ArtifactExport()
    {
    }

    public Guid ArtifactVersionId { get; private set; }

    public OutputFormat Format { get; private set; }

    public ExportStatus Status { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public static ArtifactExport Create(Guid artifactVersionId, OutputFormat format, string fileName, string content, Guid createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new ArtifactExport
        {
            ArtifactVersionId = artifactVersionId,
            Format = format,
            Status = ExportStatus.Completed,
            FileName = fileName.Trim(),
            Content = content,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }
}
