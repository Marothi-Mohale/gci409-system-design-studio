using Gci409.Api.Infrastructure;
using Gci409.Application.Recommendations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/recommendations")]
[Authorize]
public sealed class RecommendationsController(RecommendationService recommendationService) : ControllerBase
{
    [HttpGet("latest")]
    public async Task<ActionResult<RecommendationResponse>> GetLatest(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await recommendationService.GetLatestAsync(projectId, User.GetUserId(), cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<RecommendationResponse>> Generate(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await recommendationService.GenerateAsync(projectId, User.GetUserId(), cancellationToken);
        return Ok(response);
    }
}
