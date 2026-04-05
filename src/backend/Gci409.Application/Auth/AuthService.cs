using Gci409.Application.Common;
using Gci409.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Auth;

public sealed class AuthService(
    IGci409DbContext dbContext,
    IPasswordService passwordService,
    IRefreshTokenProtector refreshTokenProtector,
    IJwtTokenService jwtTokenService,
    IAuditWriter auditWriter,
    IClock clock)
{
    private const string PlatformAdminRoleName = "PlatformAdmin";

    public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new ValidationException("A user with this email address already exists.");
        }

        var isFirstUser = !await dbContext.Users.AnyAsync(cancellationToken);
        var user = User.Create(request.FullName, email, string.Empty, null, clock.UtcNow);
        user.SetPasswordHash(passwordService.HashPassword(user, request.Password), user.Id, clock.UtcNow);
        var platformRoles = new List<string>();

        await dbContext.Users.AddAsync(user, cancellationToken);

        if (isFirstUser)
        {
            var adminRole = await dbContext.Roles.SingleOrDefaultAsync(
                x => x.Name == PlatformAdminRoleName && x.Scope == RoleScope.Platform,
                cancellationToken);

            if (adminRole is null)
            {
                adminRole = Role.Create(PlatformAdminRoleName, "Bootstrap platform administrator role.", RoleScope.Platform, user.Id, clock.UtcNow);
                await dbContext.Roles.AddAsync(adminRole, cancellationToken);
            }

            var assignment = PlatformRoleAssignment.Create(user.Id, adminRole.Id, user.Id, clock.UtcNow);
            await dbContext.PlatformRoleAssignments.AddAsync(assignment, cancellationToken);
            platformRoles.Add(PlatformAdminRoleName);
        }

        var tokens = jwtTokenService.CreateTokens(user, platformRoles);
        user.IssueRefreshToken(refreshTokenProtector.Hash(tokens.RefreshToken), clock.UtcNow.AddDays(14), user.Id, clock.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(user.Id, null, "user.registered", nameof(User), user.Id.ToString(), $"Registered user {user.Email}.", cancellationToken: cancellationToken);

        return new AuthResponse(user.Id, user.FullName, user.Email, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAtUtc);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(x => x.Email == email, cancellationToken)
            ?? throw new NotFoundException("The email address or password is incorrect.");

        if (!passwordService.VerifyPassword(user, request.Password, user.PasswordHash))
        {
            throw new ValidationException("The email address or password is incorrect.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new ForbiddenException("The user account is not active.");
        }

        var platformRoles = await GetPlatformRoleNamesAsync(user.Id, cancellationToken);
        var tokens = jwtTokenService.CreateTokens(user, platformRoles);
        var nextRefreshToken = user.IssueRefreshToken(refreshTokenProtector.Hash(tokens.RefreshToken), clock.UtcNow.AddDays(14), user.Id, clock.UtcNow);
        await dbContext.RefreshTokens.AddAsync(nextRefreshToken, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(user.Id, null, "user.logged_in", nameof(User), user.Id.ToString(), $"User {user.Email} logged in.", cancellationToken: cancellationToken);

        return new AuthResponse(user.Id, user.FullName, user.Email, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAtUtc);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = refreshTokenProtector.Hash(request.RefreshToken);
        var user = await dbContext.Users
            .Include(x => x.RefreshTokens)
            .SingleOrDefaultAsync(x => x.RefreshTokens.Any(token => token.TokenHash == refreshTokenHash), cancellationToken)
            ?? throw new ValidationException("Refresh token is invalid.");

        var currentToken = user.RefreshTokens.Single(x => x.TokenHash == refreshTokenHash);
        if (currentToken.ExpiresAtUtc <= clock.UtcNow || currentToken.RevokedAtUtc is not null)
        {
            throw new ValidationException("Refresh token is no longer valid.");
        }

        currentToken.Revoke(clock.UtcNow, user.Id);
        var platformRoles = await GetPlatformRoleNamesAsync(user.Id, cancellationToken);
        var tokens = jwtTokenService.CreateTokens(user, platformRoles);
        var nextRefreshToken = user.IssueRefreshToken(refreshTokenProtector.Hash(tokens.RefreshToken), clock.UtcNow.AddDays(14), user.Id, clock.UtcNow);
        await dbContext.RefreshTokens.AddAsync(nextRefreshToken, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(user.Id, null, "user.token_refreshed", nameof(User), user.Id.ToString(), $"User {user.Email} refreshed access tokens.", cancellationToken: cancellationToken);

        return new AuthResponse(user.Id, user.FullName, user.Email, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAtUtc);
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
