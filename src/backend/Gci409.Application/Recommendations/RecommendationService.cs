using Gci409.Application.Common;
using Gci409.Application.Projects;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Projects;
using Gci409.Domain.Recommendations;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Recommendations;

public sealed class RecommendationService(
    IGci409DbContext dbContext,
    ProjectService projectService,
    IArtifactRecommendationEngine recommendationEngine,
    IAuditWriter auditWriter,
    IClock clock)
{
    public async Task<RecommendationResponse> GenerateAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Contributor, cancellationToken);

        var project = await dbContext.Projects.AsNoTracking().SingleAsync(x => x.Id == projectId, cancellationToken);
        var requirementSetVersion = await dbContext.RequirementSetVersions
            .AsNoTracking()
            .Include(x => x.Requirements)
            .Include(x => x.Constraints)
            .Where(x => x.RequirementSet.ProjectId == projectId)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ValidationException("Capture requirements before generating recommendations.");

        var drafts = recommendationEngine.Recommend(new RecommendationInput(
            project.Name,
            requirementSetVersion.Summary,
            requirementSetVersion.Requirements.Select(x => $"{x.Title}. {x.Description}").ToList(),
            requirementSetVersion.Constraints.Select(x => $"{x.Title}. {x.Description}").ToList()));

        var recommendationSet = RecommendationSet.Create(
            projectId,
            requirementSetVersion.Id,
            drafts.Select(x => Recommendation.Create((int)x.ArtifactKind, x.Title, x.Rationale, x.ConfidenceScore, x.Strength)),
            userId,
            clock.UtcNow);

        await dbContext.RecommendationSets.AddAsync(recommendationSet, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, projectId, "recommendations.generated", nameof(RecommendationSet), recommendationSet.Id.ToString(), "Generated artifact recommendations.", cancellationToken: cancellationToken);

        return new RecommendationResponse(
            recommendationSet.Id,
            recommendationSet.RequirementSetVersionId,
            recommendationSet.Items.Select(x => new RecommendationItemResponse((ArtifactKind)x.ArtifactKind, x.Title, x.Rationale, x.ConfidenceScore, x.Strength)).ToList(),
            recommendationSet.CreatedAtUtc);
    }

    public async Task<RecommendationResponse?> GetLatestAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        var recommendationSet = await dbContext.RecommendationSets
            .AsNoTracking()
            .Include(x => x.Items)
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (recommendationSet is null)
        {
            return null;
        }

        return new RecommendationResponse(
            recommendationSet.Id,
            recommendationSet.RequirementSetVersionId,
            recommendationSet.Items.Select(x => new RecommendationItemResponse((ArtifactKind)x.ArtifactKind, x.Title, x.Rationale, x.ConfidenceScore, x.Strength)).ToList(),
            recommendationSet.CreatedAtUtc);
    }
}
