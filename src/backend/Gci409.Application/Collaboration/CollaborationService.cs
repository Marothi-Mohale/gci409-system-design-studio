using Gci409.Application.Common;
using Gci409.Application.Projects;
using Gci409.Domain.Collaboration;
using Gci409.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Collaboration;

public sealed class CollaborationService(
    IGci409DbContext dbContext,
    ProjectService projectService,
    IAuditWriter auditWriter,
    IClock clock)
{
    public async Task<PagedResult<CommentThreadSummaryResponse>> ListThreadsAsync(
        Guid projectId,
        Guid userId,
        CommentTargetType? targetType,
        Guid? targetId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        var query = dbContext.CommentThreads
            .AsNoTracking()
            .Include(x => x.Comments)
            .Where(x => x.ProjectId == projectId);

        if (targetType.HasValue)
        {
            query = query.Where(x => x.TargetType == targetType.Value);
        }

        if (targetId.HasValue)
        {
            query = query.Where(x => x.TargetId == targetId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.LastModifiedAtUtc ?? x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new CommentThreadSummaryResponse(x.Id, x.TargetType, x.TargetId, x.Status, x.Comments.Count, x.CreatedAtUtc, x.LastModifiedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<CommentThreadSummaryResponse>(items, page, pageSize, totalCount);
    }

    public async Task<CommentThreadDetailResponse> GetThreadAsync(Guid projectId, Guid threadId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        var thread = await dbContext.CommentThreads
            .AsNoTracking()
            .Include(x => x.Comments)
            .SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == threadId, cancellationToken)
            ?? throw new NotFoundException("Comment thread was not found.");

        return Map(thread);
    }

    public async Task<CommentThreadDetailResponse> CreateThreadAsync(Guid projectId, Guid userId, CreateCommentThreadRequest request, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);
        await EnsureTargetBelongsToProjectAsync(projectId, request.TargetType, request.TargetId, cancellationToken);

        var existingThread = await dbContext.CommentThreads
            .Include(x => x.Comments)
            .SingleOrDefaultAsync(
                x => x.ProjectId == projectId && x.TargetType == request.TargetType && x.TargetId == request.TargetId,
                cancellationToken);

        if (existingThread is not null)
        {
            throw new ValidationException("A comment thread already exists for the selected target.");
        }

        var thread = CommentThread.Create(projectId, request.TargetType, request.TargetId, userId, clock.UtcNow);
        thread.AddComment(request.InitialComment, userId, clock.UtcNow);

        await dbContext.CommentThreads.AddAsync(thread, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, projectId, "comment.thread_created", nameof(CommentThread), thread.Id.ToString(), $"Created comment thread for {request.TargetType}.", cancellationToken: cancellationToken);

        return Map(thread);
    }

    public async Task<CommentThreadDetailResponse> AddCommentAsync(Guid projectId, Guid threadId, Guid userId, AddCommentRequest request, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        var thread = await dbContext.CommentThreads
            .Include(x => x.Comments)
            .SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == threadId, cancellationToken)
            ?? throw new NotFoundException("Comment thread was not found.");

        var comment = thread.AddComment(request.Body, userId, clock.UtcNow);
        await dbContext.Comments.AddAsync(comment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, projectId, "comment.added", nameof(CommentThread), thread.Id.ToString(), "Added comment to thread.", cancellationToken: cancellationToken);

        return Map(thread);
    }

    public async Task<CommentThreadDetailResponse> ResolveThreadAsync(Guid projectId, Guid threadId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Reviewer, cancellationToken);

        var thread = await dbContext.CommentThreads
            .Include(x => x.Comments)
            .SingleOrDefaultAsync(x => x.ProjectId == projectId && x.Id == threadId, cancellationToken)
            ?? throw new NotFoundException("Comment thread was not found.");

        thread.Resolve(userId, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, projectId, "comment.thread_resolved", nameof(CommentThread), thread.Id.ToString(), "Resolved comment thread.", cancellationToken: cancellationToken);

        return Map(thread);
    }

    private async Task EnsureTargetBelongsToProjectAsync(Guid projectId, CommentTargetType targetType, Guid targetId, CancellationToken cancellationToken)
    {
        var exists = targetType switch
        {
            CommentTargetType.Project => await dbContext.Projects.AnyAsync(x => x.Id == targetId && x.Id == projectId, cancellationToken),
            CommentTargetType.RequirementSetVersion => await dbContext.RequirementSetVersions.AnyAsync(x => x.Id == targetId && x.RequirementSet.ProjectId == projectId, cancellationToken),
            CommentTargetType.RecommendationSet => await dbContext.RecommendationSets.AnyAsync(x => x.Id == targetId && x.ProjectId == projectId, cancellationToken),
            CommentTargetType.GeneratedArtifact => await dbContext.GeneratedArtifacts.AnyAsync(x => x.Id == targetId && x.ProjectId == projectId, cancellationToken),
            CommentTargetType.ArtifactVersion => await dbContext.ArtifactVersions.AnyAsync(x => x.Id == targetId && x.GeneratedArtifact.ProjectId == projectId, cancellationToken),
            _ => false
        };

        if (!exists)
        {
            throw new ValidationException("The selected comment target does not belong to this project.");
        }
    }

    private static CommentThreadDetailResponse Map(CommentThread thread)
    {
        return new CommentThreadDetailResponse(
            thread.Id,
            thread.TargetType,
            thread.TargetId,
            thread.Status,
            thread.Comments
                .OrderBy(x => x.CreatedAtUtc)
                .Select(x => new CommentResponse(x.Id, x.CreatedByUserId, x.Body, x.CreatedAtUtc))
                .ToList(),
            thread.CreatedAtUtc,
            thread.LastModifiedAtUtc);
    }
}
