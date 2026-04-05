using FluentValidation;
using Gci409.Domain.Identity;

namespace Gci409.Application.Profile;

public sealed record CurrentUserProfileResponse(Guid Id, string FullName, string Email, UserStatus Status, IReadOnlyCollection<string> PlatformRoles, DateTimeOffset CreatedAtUtc);

public sealed record UpdateCurrentUserProfileRequest(string FullName);

public sealed class UpdateCurrentUserProfileRequestValidator : AbstractValidator<UpdateCurrentUserProfileRequest>
{
    public UpdateCurrentUserProfileRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
    }
}
