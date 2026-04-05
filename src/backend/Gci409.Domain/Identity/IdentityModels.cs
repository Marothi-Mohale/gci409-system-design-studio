using Gci409.Domain.Common;

namespace Gci409.Domain.Identity;

public enum UserStatus
{
    Pending = 1,
    Active = 2,
    Suspended = 3,
    Disabled = 4
}

public enum RoleScope
{
    Platform = 1,
    Project = 2
}

public sealed class User : AuditableEntity, IAggregateRoot
{
    private readonly List<RefreshToken> _refreshTokens = [];
    private readonly List<PlatformRoleAssignment> _platformRoleAssignments = [];

    private User()
    {
    }

    public string FullName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public UserStatus Status { get; private set; } = UserStatus.Active;

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    public IReadOnlyCollection<PlatformRoleAssignment> PlatformRoleAssignments => _platformRoleAssignments;

    public static User Create(string fullName, string email, string passwordHash, Guid? createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new User
        {
            FullName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Status = UserStatus.Active,
            CreatedAtUtc = createdAtUtc,
            CreatedByUserId = createdByUserId
        };
    }

    public void UpdateProfile(string fullName, Guid? modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        FullName = fullName.Trim();
        Touch(modifiedByUserId, modifiedAtUtc);
    }

    public void SetPasswordHash(string passwordHash, Guid? modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        PasswordHash = passwordHash;
        Touch(modifiedByUserId, modifiedAtUtc);
    }

    public void SetStatus(UserStatus status, Guid? modifiedByUserId, DateTimeOffset modifiedAtUtc)
    {
        Status = status;
        Touch(modifiedByUserId, modifiedAtUtc);
    }

    public RefreshToken IssueRefreshToken(string tokenHash, DateTimeOffset expiresAtUtc, Guid? createdByUserId, DateTimeOffset createdAtUtc)
    {
        var refreshToken = RefreshToken.Create(Id, tokenHash, expiresAtUtc, createdByUserId, createdAtUtc);
        _refreshTokens.Add(refreshToken);
        return refreshToken;
    }
}

public sealed class RefreshToken : AuditableEntity
{
    private RefreshToken()
    {
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public static RefreshToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAtUtc, Guid? createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAtUtc = expiresAtUtc,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }

    public void Revoke(DateTimeOffset revokedAtUtc, Guid? revokedByUserId)
    {
        RevokedAtUtc = revokedAtUtc;
        Touch(revokedByUserId, revokedAtUtc);
    }
}

public sealed class Role : AuditableEntity, IAggregateRoot
{
    private readonly List<RolePermission> _permissions = [];

    private Role()
    {
    }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public RoleScope Scope { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions;

    public static Role Create(string name, string description, RoleScope scope, Guid? createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new Role
        {
            Name = name.Trim(),
            Description = description.Trim(),
            Scope = scope,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }
}

public sealed class Permission : AuditableEntity, IAggregateRoot
{
    private Permission()
    {
    }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public static Permission Create(string code, string description, Guid? createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new Permission
        {
            Code = code.Trim(),
            Description = description.Trim(),
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }
}

public sealed class RolePermission : Entity
{
    private RolePermission()
    {
    }

    public Guid RoleId { get; private set; }

    public Guid PermissionId { get; private set; }

    public static RolePermission Create(Guid roleId, Guid permissionId)
    {
        return new RolePermission
        {
            RoleId = roleId,
            PermissionId = permissionId
        };
    }
}

public sealed class PlatformRoleAssignment : AuditableEntity
{
    private PlatformRoleAssignment()
    {
    }

    public Guid UserId { get; private set; }

    public Guid RoleId { get; private set; }

    public static PlatformRoleAssignment Create(Guid userId, Guid roleId, Guid? createdByUserId, DateTimeOffset createdAtUtc)
    {
        return new PlatformRoleAssignment
        {
            UserId = userId,
            RoleId = roleId,
            CreatedByUserId = createdByUserId,
            CreatedAtUtc = createdAtUtc
        };
    }
}
