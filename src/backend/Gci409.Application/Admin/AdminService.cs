using Gci409.Application.Common;
using Gci409.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Admin;

public sealed class AdminService(IGci409DbContext dbContext, IAuditWriter auditWriter, IClock clock)
{
    private const string PlatformAdministratorRoleName = "PlatformAdmin";

    public async Task EnsurePlatformAdministratorAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var hasAccess = await dbContext.PlatformRoleAssignments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(dbContext.Roles, assignment => assignment.RoleId, role => role.Id, (_, role) => role.Name)
            .AnyAsync(roleName => roleName == PlatformAdministratorRoleName, cancellationToken);

        if (!hasAccess)
        {
            throw new ForbiddenException("Platform administrator access is required.");
        }
    }

    public async Task<PagedResult<AdminUserSummaryResponse>> GetUsersAsync(Guid actorUserId, int page, int pageSize, string? search, UserStatus? status, CancellationToken cancellationToken = default)
    {
        await EnsurePlatformAdministratorAsync(actorUserId, cancellationToken);

        var query = dbContext.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Email.ToLower().Contains(normalized) || x.FullName.ToLower().Contains(normalized));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderBy(x => x.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.Id,
                x.FullName,
                x.Email,
                x.Status,
                x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var roleLookup = await dbContext.PlatformRoleAssignments
            .AsNoTracking()
            .Join(dbContext.Roles, assignment => assignment.RoleId, role => role.Id, (assignment, role) => new { assignment.UserId, role.Name })
            .GroupBy(x => x.UserId)
            .ToDictionaryAsync(x => x.Key, x => (IReadOnlyCollection<string>)x.Select(y => y.Name).OrderBy(y => y).ToList(), cancellationToken);

        var items = users
            .Select(x => new AdminUserSummaryResponse(x.Id, x.FullName, x.Email, x.Status, roleLookup.GetValueOrDefault(x.Id, Array.Empty<string>()), x.CreatedAtUtc))
            .ToList();

        return new PagedResult<AdminUserSummaryResponse>(items, page, pageSize, totalCount);
    }

    public async Task<AdminUserSummaryResponse> UpdateUserStatusAsync(Guid actorUserId, Guid targetUserId, UpdateUserStatusRequest request, CancellationToken cancellationToken = default)
    {
        await EnsurePlatformAdministratorAsync(actorUserId, cancellationToken);

        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Id == targetUserId, cancellationToken)
            ?? throw new NotFoundException("User was not found.");

        user.SetStatus(request.Status, actorUserId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(actorUserId, null, "admin.user_status_updated", nameof(User), user.Id.ToString(), $"Set user {user.Email} status to {request.Status}.", cancellationToken: cancellationToken);

        var roles = await GetRoleNamesAsync(targetUserId, cancellationToken);
        return new AdminUserSummaryResponse(user.Id, user.FullName, user.Email, user.Status, roles, user.CreatedAtUtc);
    }

    public async Task<IReadOnlyCollection<RoleSummaryResponse>> GetRolesAsync(Guid actorUserId, CancellationToken cancellationToken = default)
    {
        await EnsurePlatformAdministratorAsync(actorUserId, cancellationToken);

        var roles = await dbContext.Roles
            .AsNoTracking()
            .OrderBy(x => x.Scope)
            .ThenBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.Scope
            })
            .ToListAsync(cancellationToken);

        var permissionLookup = await dbContext.RolePermissions
            .AsNoTracking()
            .Join(dbContext.Permissions, assignment => assignment.PermissionId, permission => permission.Id, (assignment, permission) => new { assignment.RoleId, permission.Code })
            .GroupBy(x => x.RoleId)
            .ToDictionaryAsync(x => x.Key, x => (IReadOnlyCollection<string>)x.Select(y => y.Code).OrderBy(y => y).ToList(), cancellationToken);

        return roles
            .Select(x => new RoleSummaryResponse(x.Id, x.Name, x.Description, x.Scope, permissionLookup.GetValueOrDefault(x.Id, Array.Empty<string>())))
            .ToList();
    }

    private async Task<IReadOnlyCollection<string>> GetRoleNamesAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.PlatformRoleAssignments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Join(dbContext.Roles, assignment => assignment.RoleId, role => role.Id, (_, role) => role.Name)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }
}
