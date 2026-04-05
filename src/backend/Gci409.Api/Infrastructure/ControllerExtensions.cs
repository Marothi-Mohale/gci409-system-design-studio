using System.Security.Claims;

namespace Gci409.Api.Infrastructure;

public static class ControllerExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue("sub");
        return Guid.TryParse(subject, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Authenticated user id was not found.");
    }
}
