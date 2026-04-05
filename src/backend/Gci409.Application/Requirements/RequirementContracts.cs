using Gci409.Domain.Requirements;

namespace Gci409.Application.Requirements;

public sealed record RequirementInput(string Code, string Title, string Description, RequirementType Type, RequirementPriority Priority);

public sealed record ConstraintInput(string Title, string Description, ConstraintType Type, ConstraintSeverity Severity);

public sealed record SaveRequirementSetRequest(string Name, string Summary, IReadOnlyCollection<RequirementInput> Requirements, IReadOnlyCollection<ConstraintInput> Constraints);

public sealed record RequirementSetVersionResponse(
    Guid RequirementSetId,
    Guid VersionId,
    string Name,
    int VersionNumber,
    string Summary,
    IReadOnlyCollection<RequirementInput> Requirements,
    IReadOnlyCollection<ConstraintInput> Constraints,
    DateTimeOffset CreatedAtUtc);
