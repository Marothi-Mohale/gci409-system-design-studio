using Gci409.Api.Infrastructure;
using Gci409.Application.Templates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Authorize]
public sealed class TemplatesController(TemplateService templateService) : ControllerBase
{
    [HttpGet("api/projects/{projectId:guid}/templates")]
    public async Task<ActionResult<IReadOnlyCollection<TemplateSummaryResponse>>> ListForProject(Guid projectId, CancellationToken cancellationToken)
    {
        var response = await templateService.ListForProjectAsync(projectId, User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpPost("api/projects/{projectId:guid}/templates")]
    public async Task<ActionResult<TemplateDetailResponse>> Create(Guid projectId, CreateTemplateRequest request, CancellationToken cancellationToken)
    {
        var response = await templateService.CreateForProjectAsync(projectId, User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { templateId = response.Id }, response);
    }

    [HttpGet("api/templates/{templateId:guid}")]
    public async Task<ActionResult<TemplateDetailResponse>> GetById(Guid templateId, CancellationToken cancellationToken)
    {
        var response = await templateService.GetAsync(templateId, User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("api/templates/{templateId:guid}/versions")]
    public async Task<ActionResult<IReadOnlyCollection<TemplateVersionResponse>>> GetVersions(Guid templateId, CancellationToken cancellationToken)
    {
        var response = await templateService.GetAsync(templateId, User.GetUserId(), cancellationToken);
        return Ok(response.Versions);
    }

    [HttpPost("api/templates/{templateId:guid}/versions")]
    public async Task<ActionResult<TemplateVersionResponse>> CreateVersion(Guid templateId, CreateTemplateVersionRequest request, CancellationToken cancellationToken)
    {
        var response = await templateService.CreateVersionAsync(templateId, User.GetUserId(), request, cancellationToken);
        return Ok(response);
    }
}
