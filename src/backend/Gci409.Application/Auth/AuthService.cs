using Gci409.Application.Common;
using Gci409.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Auth;

public sealed class AuthService(
    IGci409DbContext dbContext,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService,
    IAuditWriter auditWriter,
    IClock clock)
{
    public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(x => x.Email == email, cancellationToken))
        {
            throw new ValidationException("A user with this email address already exists.");
        }

        var user = User.Create(request.FullName, email, string.Empty, null, clock.UtcNow);
        user.SetPasswordHash(passwordService.HashPassword(user, request.Password), user.Id, clock.UtcNow);

        await dbContext.Users.AddAsync(user, cancellationToken);
        var tokens = jwtTokenService.CreateTokens(user);
        user.IssueRefreshToken(tokens.RefreshToken, clock.UtcNow.AddDays(14), user.Id, clock.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(user.Id, null, "user.registered", nameof(User), user.Id.ToString(), $"Registered user {user.Email}.", cancellationToken: cancellationToken);

        return new AuthResponse(user.Id, user.FullName, user.Email, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAtUtc);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.Include(x => x.RefreshTokens).SingleOrDefaultAsync(x => x.Email == email, cancellationToken)
            ?? throw new NotFoundException("The email address or password is incorrect.");

        if (!passwordService.VerifyPassword(user, request.Password, user.PasswordHash))
        {
            throw new ValidationException("The email address or password is incorrect.");
        }

        var tokens = jwtTokenService.CreateTokens(user);
        user.IssueRefreshToken(tokens.RefreshToken, clock.UtcNow.AddDays(14), user.Id, clock.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(user.Id, null, "user.logged_in", nameof(User), user.Id.ToString(), $"User {user.Email} logged in.", cancellationToken: cancellationToken);

        return new AuthResponse(user.Id, user.FullName, user.Email, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAtUtc);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .Include(x => x.RefreshTokens)
            .SingleOrDefaultAsync(x => x.RefreshTokens.Any(token => token.TokenHash == request.RefreshToken), cancellationToken)
            ?? throw new ValidationException("Refresh token is invalid.");

        var currentToken = user.RefreshTokens.Single(x => x.TokenHash == request.RefreshToken);
        if (currentToken.ExpiresAtUtc <= clock.UtcNow || currentToken.RevokedAtUtc is not null)
        {
            throw new ValidationException("Refresh token is no longer valid.");
        }

        currentToken.Revoke(clock.UtcNow, user.Id);
        var tokens = jwtTokenService.CreateTokens(user);
        user.IssueRefreshToken(tokens.RefreshToken, clock.UtcNow.AddDays(14), user.Id, clock.UtcNow);

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(user.Id, null, "user.token_refreshed", nameof(User), user.Id.ToString(), $"User {user.Email} refreshed access tokens.", cancellationToken: cancellationToken);

        return new AuthResponse(user.Id, user.FullName, user.Email, tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresAtUtc);
    }
}
