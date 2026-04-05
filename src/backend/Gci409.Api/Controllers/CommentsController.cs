using Gci409.Api.Infrastructure;
using Gci409.Application.Collaboration;
using Gci409.Application.Common;
using Gci409.Domain.Collaboration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gci409.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/comments/threads")]
[Authorize]
public sealed class CommentsController(CollaborationService collaborationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<CommentThreadSummaryResponse>>> List(
        Guid projectId,
        [FromQuery] CommentTargetType? targetType,
        [FromQuery] Guid? targetId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await collaborationService.ListThreadsAsync(
            projectId,
            User.GetUserId(),
            targetType,
            targetId,
            Math.Max(page, 1),
            Math.Clamp(pageSize, 1, 100),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{threadId:guid}")]
    public async Task<ActionResult<CommentThreadDetailResponse>> Get(Guid projectId, Guid threadId, CancellationToken cancellationToken)
    {
        var response = await collaborationService.GetThreadAsync(projectId, threadId, User.GetUserId(), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<CommentThreadDetailResponse>> Create(Guid projectId, CreateCommentThreadRequest request, CancellationToken cancellationToken)
    {
        var response = await collaborationService.CreateThreadAsync(projectId, User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { projectId, threadId = response.Id }, response);
    }

    [HttpPost("{threadId:guid}/comments")]
    public async Task<ActionResult<CommentThreadDetailResponse>> AddComment(Guid projectId, Guid threadId, AddCommentRequest request, CancellationToken cancellationToken)
    {
        var response = await collaborationService.AddCommentAsync(projectId, threadId, User.GetUserId(), request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("{threadId:guid}/resolve")]
    public async Task<ActionResult<CommentThreadDetailResponse>> Resolve(Guid projectId, Guid threadId, CancellationToken cancellationToken)
    {
        var response = await collaborationService.ResolveThreadAsync(projectId, threadId, User.GetUserId(), cancellationToken);
        return Ok(response);
    }
}
