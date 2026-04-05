using Gci409.Api.Infrastructure;
using Gci409.Application.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public sealed class ProjectsController(ProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProjectSummary>>> GetMine(CancellationToken cancellationToken)
    {
        var projects = await projectService.GetForUserAsync(User.GetUserId(), cancellationToken);
        return Ok(projects);
    }

    [HttpPost]
    public async Task<ActionResult<ProjectSummary>> Create(CreateProjectRequest request, CancellationToken cancellationToken)
    {
        var project = await projectService.CreateAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { projectId = project.Id }, project);
    }

    [HttpGet("{projectId:guid}")]
    public async Task<ActionResult<ProjectDetail>> GetById(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await projectService.GetAsync(projectId, User.GetUserId(), cancellationToken);
        return Ok(project);
    }
}
