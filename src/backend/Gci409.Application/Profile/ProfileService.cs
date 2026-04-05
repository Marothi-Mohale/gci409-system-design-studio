using Gci409.Application.Common;
using Gci409.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Profile;

public sealed class ProfileService(IGci409DbContext dbContext, IAuditWriter auditWriter, IClock clock)
{
    public async Task<CurrentUserProfileResponse> GetCurrentAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User profile was not found.");

        var platformRoles = await GetPlatformRoleNamesAsync(userId, cancellationToken);
        return new CurrentUserProfileResponse(user.Id, user.FullName, user.Email, user.Status, platformRoles, user.CreatedAtUtc);
    }

    public async Task<CurrentUserProfileResponse> UpdateCurrentAsync(Guid userId, UpdateCurrentUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User profile was not found.");

        user.UpdateProfile(request.FullName, userId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, null, "profile.updated", nameof(User), user.Id.ToString(), $"Updated profile for {user.Email}.", cancellationToken: cancellationToken);

        var platformRoles = await GetPlatformRoleNamesAsync(userId, cancellationToken);
        return new CurrentUserProfileResponse(user.Id, user.FullName, user.Email, user.Status, platformRoles, user.CreatedAtUtc);
    }

    private async Task<IReadOnlyCollection<string>> GetPlatformRoleNamesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.PlatformRoleAssignments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(dbContext.Roles, assignment => assignment.RoleId, role => role.Id, (_, role) => role.Name)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }
}
