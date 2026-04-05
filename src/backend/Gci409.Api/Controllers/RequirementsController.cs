using Gci409.Api.Infrastructure;
using Gci409.Application.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/requirements")]
[Authorize]
public sealed class RequirementsController(RequirementService requirementService) : ControllerBase
{
    [HttpGet("current")]
    public async Task<ActionResult<RequirementSetVersionResponse>> GetCurrent(Guid projectId, CancellationToken cancellationToken)
    {
        var requirementSet = await requirementService.GetCurrentAsync(projectId, User.GetUserId(), cancellationToken);
        return requirementSet is null ? NotFound() : Ok(requirementSet);
    }

    [HttpPost]
    public async Task<ActionResult<RequirementSetVersionResponse>> Save(Guid projectId, SaveRequirementSetRequest request, CancellationToken cancellationToken)
    {
        var response = await requirementService.SaveCurrentAsync(projectId, User.GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("bootstrap")]
    public async Task<ActionResult<RequirementSetVersionResponse>> Bootstrap(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await requirementService.BootstrapFromProjectBriefAsync(projectId, User.GetUserId(), cancellationToken);
        return Ok(response);
    }
}
