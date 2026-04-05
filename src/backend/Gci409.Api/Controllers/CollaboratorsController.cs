using Gci409.Api.Infrastructure;
using Gci409.Application.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/collaborators")]
[Authorize]
public sealed class CollaboratorsController(ProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProjectMemberSummary>>> List(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await projectService.GetCollaboratorsAsync(projectId, User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectMemberSummary>> Add(Guid projectId, AddCollaboratorRequest request, CancellationToken cancellationToken)
    {
        var response = await projectService.AddCollaboratorAsync(projectId, User.GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [HttpPatch("{userId:guid}")]
    public async Task<ActionResult<ProjectMemberSummary>> Update(Guid projectId, Guid userId, UpdateCollaboratorRequest request, CancellationToken cancellationToken)
    {
        var response = await projectService.UpdateCollaboratorAsync(projectId, userId, User.GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Remove(Guid projectId, Guid userId, CancellationToken cancellationToken)
    {
        await projectService.RemoveCollaboratorAsync(projectId, userId, User.GetUserId(), cancellationToken);
        return NoContent();
    }
}
