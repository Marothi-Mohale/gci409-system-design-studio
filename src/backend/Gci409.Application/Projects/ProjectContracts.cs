using FluentValidation;
using Gci409.Domain.Projects;

namespace Gci409.Application.Projects;

public sealed record CreateProjectRequest(string Name, string Description);

public sealed record UpdateProjectRequest(string Name, string Description);

public sealed record AddCollaboratorRequest(Guid UserId, ProjectRole Role);

public sealed record UpdateCollaboratorRequest(ProjectRole Role);

public sealed record ProjectSummary(Guid Id, string Key, string Name, string Description, ProjectStatus Status, ProjectRole Role, DateTimeOffset CreatedAtUtc);

public sealed record ProjectDetail(Guid Id, string Key, string Name, string Description, ProjectStatus Status, IReadOnlyCollection<ProjectMemberSummary> Members);

public sealed record ProjectMemberSummary(Guid UserId, ProjectRole Role, MembershipStatus Status);

public sealed class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}

public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
    }
}

public sealed class AddCollaboratorRequestValidator : AbstractValidator<AddCollaboratorRequest>
{
    public AddCollaboratorRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum().Must(x => x != ProjectRole.Owner)
            .WithMessage("Use project creation to establish ownership.");
    }
}

public sealed class UpdateCollaboratorRequestValidator : AbstractValidator<UpdateCollaboratorRequest>
{
    public UpdateCollaboratorRequestValidator()
    {
        RuleFor(x => x.Role).IsInEnum().Must(x => x != ProjectRole.Owner)
            .WithMessage("Ownership changes are not supported through this endpoint.");
    }
}
