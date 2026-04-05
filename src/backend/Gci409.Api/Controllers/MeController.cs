using Gci409.Api.Infrastructure;
using Gci409.Application.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController(ProfileService profileService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CurrentUserProfileResponse>> Get(CancellationToken cancellationToken)
    {
        var response = await profileService.GetCurrentAsync(User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpPatch]
    public async Task<ActionResult<CurrentUserProfileResponse>> Update(UpdateCurrentUserProfileRequest request, CancellationToken cancellationToken)
    {
        var response = await profileService.UpdateCurrentAsync(User.GetUserId(), request, cancellationToken);
        return Ok(response);
    }
}
