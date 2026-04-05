using FluentValidation;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Templates;

namespace Gci409.Application.Templates;

public sealed record CreateTemplateRequest(string Name, string Description, IReadOnlyCollection<ArtifactKind> ArtifactKinds, string Content);

public sealed record CreateTemplateVersionRequest(IReadOnlyCollection<ArtifactKind> ArtifactKinds, string Content);

public sealed record TemplateSummaryResponse(Guid Id, Guid? ProjectId, string Name, string Description, TemplateStatus Status, int CurrentVersionNumber, DateTimeOffset CreatedAtUtc);

public sealed record TemplateVersionResponse(Guid Id, int VersionNumber, string Content, IReadOnlyCollection<ArtifactKind> ArtifactKinds, DateTimeOffset CreatedAtUtc);

public sealed record TemplateDetailResponse(Guid Id, Guid? ProjectId, string Name, string Description, TemplateStatus Status, int CurrentVersionNumber, IReadOnlyCollection<TemplateVersionResponse> Versions, DateTimeOffset CreatedAtUtc);

public sealed class CreateTemplateRequestValidator : AbstractValidator<CreateTemplateRequest>
{
    public CreateTemplateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.ArtifactKinds).NotEmpty();
        RuleForEach(x => x.ArtifactKinds).IsInEnum();
    }
}

public sealed class CreateTemplateVersionRequestValidator : AbstractValidator<CreateTemplateVersionRequest>
{
    public CreateTemplateVersionRequestValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.ArtifactKinds).NotEmpty();
        RuleForEach(x => x.ArtifactKinds).IsInEnum();
    }
}
