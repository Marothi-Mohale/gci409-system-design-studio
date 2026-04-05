using FluentValidation;
using Gci409.Domain.Identity;

namespace Gci409.Application.Admin;

public sealed record AdminUserSummaryResponse(Guid Id, string FullName, string Email, UserStatus Status, IReadOnlyCollection<string> PlatformRoles, DateTimeOffset CreatedAtUtc);

public sealed record UpdateUserStatusRequest(UserStatus Status);

public sealed record RoleSummaryResponse(Guid Id, string Name, string Description, RoleScope Scope, IReadOnlyCollection<string> PermissionCodes);

public sealed class UpdateUserStatusRequestValidator : AbstractValidator<UpdateUserStatusRequest>
{
    public UpdateUserStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
