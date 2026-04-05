using System.Text;
using Gci409.Api.Infrastructure;
using Gci409.Application.Common;
using Gci409.Application.Exports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Authorize]
public sealed class ExportsController(ExportService exportService) : ControllerBase
{
    [HttpGet("api/projects/{projectId:guid}/exports")]
    public async Task<ActionResult<PagedResult<ExportSummaryResponse>>> List(
        Guid projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await exportService.ListAsync(projectId, User.GetUserId(), Math.Max(page, 1), Math.Clamp(pageSize, 1, 100), cancellationToken);
        return Ok(response);
    }

    [HttpGet("api/exports/{exportId:guid}")]
    public async Task<ActionResult<ExportDetailResponse>> Get(Guid exportId, CancellationToken cancellationToken)
    {
        var response = await exportService.GetAsync(exportId, User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("api/exports/{exportId:guid}/download")]
    public async Task<IActionResult> Download(Guid exportId, CancellationToken cancellationToken)
    {
        var response = await exportService.GetAsync(exportId, User.GetUserId(), cancellationToken);
        return File(Encoding.UTF8.GetBytes(response.Content), ResolveContentType(response.FileName), response.FileName);
    }

    private static string ResolveContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".md" => "text/markdown",
            ".mmd" => "text/plain",
            ".puml" => "text/plain",
            ".json" => "application/json",
            _ => "text/plain"
        };
    }
}
