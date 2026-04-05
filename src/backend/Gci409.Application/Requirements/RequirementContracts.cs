using FluentValidation;
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

public sealed class RequirementInputValidator : AbstractValidator<RequirementInput>
{
    public RequirementInputValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Priority).IsInEnum();
    }
}

public sealed class ConstraintInputValidator : AbstractValidator<ConstraintInput>
{
    public ConstraintInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Severity).IsInEnum();
    }
}

public sealed class SaveRequirementSetRequestValidator : AbstractValidator<SaveRequirementSetRequest>
{
    public SaveRequirementSetRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Requirements).NotEmpty();
        RuleForEach(x => x.Requirements).SetValidator(new RequirementInputValidator());
        RuleForEach(x => x.Constraints).SetValidator(new ConstraintInputValidator());
        RuleFor(x => x.Requirements)
            .Must(requirements => requirements.Select(x => x.Code.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count() == requirements.Count)
            .WithMessage("Requirement codes must be unique within a requirement set version.");
    }
}
