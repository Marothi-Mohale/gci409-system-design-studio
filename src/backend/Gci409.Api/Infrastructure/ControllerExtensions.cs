using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Gci409.Api.Infrastructure;

public static class ControllerExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var subject =
            principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(subject, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Authenticated user id was not found.");
    }
}
