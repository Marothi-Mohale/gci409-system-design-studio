using Gci409.Api.Infrastructure;
using Gci409.Application.Artifacts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Authorize]
public sealed class ArtifactsController(ArtifactService artifactService) : ControllerBase
{
    [HttpGet("api/projects/{projectId:guid}/artifacts")]
    public async Task<ActionResult<IReadOnlyCollection<ArtifactSummaryResponse>>> List(Guid projectId, CancellationToken cancellationToken)
    {
        var artifacts = await artifactService.ListAsync(projectId, User.GetUserId(), cancellationToken);
        return Ok(artifacts);
    }

    [HttpGet("api/projects/{projectId:guid}/artifacts/{artifactId:guid}/versions")]
    public async Task<ActionResult<IReadOnlyCollection<ArtifactVersionResponse>>> Versions(Guid projectId, Guid artifactId, CancellationToken cancellationToken)
    {
        var versions = await artifactService.GetVersionsAsync(projectId, artifactId, User.GetUserId(), cancellationToken);
        return Ok(versions);
    }

    [HttpPost("api/artifact-versions/{artifactVersionId:guid}/exports")]
    public async Task<ActionResult<ExportResponse>> Export(Guid artifactVersionId, CreateExportRequest request, CancellationToken cancellationToken)
    {
        var response = await artifactService.ExportAsync(artifactVersionId, User.GetUserId(), request, cancellationToken);
        return Ok(response);
    }
}
