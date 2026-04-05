using Gci409.Api.Infrastructure;
using Gci409.Application.Generation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/generation-requests")]
[Authorize]
public sealed class GenerationRequestsController(GenerationService generationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<GenerationRequestResponse>>> List(Guid projectId, CancellationToken cancellationToken)
    {
        var requests = await generationService.ListAsync(projectId, User.GetUserId(), cancellationToken);
        return Ok(requests);
    }

    [HttpPost]
    public async Task<ActionResult<GenerationRequestResponse>> Queue(Guid projectId, QueueGenerationRequest request, CancellationToken cancellationToken)
    {
        var response = await generationService.QueueAsync(projectId, User.GetUserId(), request, cancellationToken);
        return Accepted(response);
    }
}
