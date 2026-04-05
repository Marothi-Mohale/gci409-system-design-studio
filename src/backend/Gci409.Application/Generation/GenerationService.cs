using Gci409.Application.Common;
using Gci409.Application.Projects;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Generation;
using Gci409.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Generation;

public sealed class GenerationService(
    IGci409DbContext dbContext,
    ProjectService projectService,
    IArtifactGenerationEngine artifactGenerationEngine,
    IAuditWriter auditWriter,
    IClock clock)
{
    public async Task<GenerationRequestResponse> QueueAsync(Guid projectId, Guid userId, QueueGenerationRequest request, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Contributor, cancellationToken);

        var currentRequirementVersion = await dbContext.RequirementSetVersions
            .AsNoTracking()
            .Where(x => x.RequirementSet.ProjectId == projectId)
            .OrderByDescending(x => x.VersionNumber)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ValidationException("Save a requirement set before generating artifacts.");

        var targets = request.ArtifactKinds
            .Distinct()
            .Select(kind => GenerationRequestTarget.Create(kind, request.PreferredFormat))
            .ToList();

        var generationRequest = GenerationRequest.Create(projectId, currentRequirementVersion.Id, targets, userId, clock.UtcNow);
        await dbContext.GenerationRequests.AddAsync(generationRequest, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await auditWriter.WriteAsync(userId, projectId, "generation.queued", nameof(GenerationRequest), generationRequest.Id.ToString(), "Queued artifact generation.", cancellationToken: cancellationToken);

        return Map(generationRequest);
    }

    public async Task<IReadOnlyCollection<GenerationRequestResponse>> ListAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        await projectService.EnsureProjectAccessAsync(projectId, userId, ProjectRole.Viewer, cancellationToken);

        var requests = await dbContext.GenerationRequests
            .AsNoTracking()
            .Include(x => x.Targets)
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return requests.Select(Map).ToList();
    }

    public async Task<GenerationRequestResponse?> ProcessNextQueuedAsync(CancellationToken cancellationToken = default)
    {
        var request = await dbContext.GenerationRequests
            .Include(x => x.Targets)
            .OrderBy(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(x => x.Status == GenerationRequestStatus.Queued, cancellationToken);

        if (request is null)
        {
            return null;
        }

        var project = await dbContext.Projects.AsNoTracking().SingleAsync(x => x.Id == request.ProjectId, cancellationToken);
        var requirementVersion = await dbContext.RequirementSetVersions
            .Include(x => x.Requirements)
            .Include(x => x.Constraints)
            .SingleAsync(x => x.Id == request.RequirementSetVersionId, cancellationToken);

        request.MarkProcessing(request.CreatedByUserId ?? Guid.Empty, clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var drafts = artifactGenerationEngine.Generate(new ArtifactGenerationInput(
                project.Name,
                requirementVersion.Summary,
                requirementVersion.Requirements.Select(x => $"{x.Title}. {x.Description}").ToList(),
                requirementVersion.Constraints.Select(x => $"{x.Title}. {x.Description}").ToList(),
                request.Targets.Select(x => x.ArtifactKind).ToList()));

            foreach (var draft in drafts)
            {
                var artifact = await dbContext.GeneratedArtifacts
                    .Include(x => x.Versions)
                    .SingleOrDefaultAsync(x => x.ProjectId == request.ProjectId && x.ArtifactKind == draft.ArtifactKind, cancellationToken);

                var isNew = artifact is null;
                artifact ??= GeneratedArtifact.Create(request.ProjectId, draft.ArtifactKind, draft.Title, request.CreatedByUserId ?? Guid.Empty, clock.UtcNow);
                if (isNew)
                {
                    await dbContext.GeneratedArtifacts.AddAsync(artifact, cancellationToken);
                }

                var version = artifact.AddVersion(draft.PrimaryFormat, draft.Summary, draft.Content, draft.RepresentationsJson, request.Id, request.CreatedByUserId ?? Guid.Empty, clock.UtcNow);
                await dbContext.ArtifactVersions.AddAsync(version, cancellationToken);
                if (draft.DiagramType != UmlDiagramType.None)
                {
                    var hadUmlProfile = artifact.UmlProfile is not null;
                    artifact.EnsureUmlProfile(draft.DiagramType);
                    if (!hadUmlProfile && !isNew && artifact.UmlProfile is not null)
                    {
                        await dbContext.UmlArtifactProfiles.AddAsync(artifact.UmlProfile, cancellationToken);
                    }
                }
            }

            request.MarkCompleted(request.CreatedByUserId ?? Guid.Empty, clock.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditWriter.WriteAsync(request.CreatedByUserId, request.ProjectId, "generation.completed", nameof(GenerationRequest), request.Id.ToString(), "Artifact generation completed.", cancellationToken: cancellationToken);
            return Map(request);
        }
        catch (Exception ex)
        {
            request.MarkFailed(ex.Message, request.CreatedByUserId ?? Guid.Empty, clock.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
            await auditWriter.WriteAsync(request.CreatedByUserId, request.ProjectId, "generation.failed", nameof(GenerationRequest), request.Id.ToString(), $"Artifact generation failed: {ex.Message}", cancellationToken: cancellationToken);
            throw;
        }
    }

    private static GenerationRequestResponse Map(GenerationRequest request)
    {
        return new GenerationRequestResponse(
            request.Id,
            request.RequirementSetVersionId,
            request.Status,
            request.Targets.Select(x => new GenerationTargetResponse(x.ArtifactKind, x.PreferredFormat)).ToList(),
            request.CreatedAtUtc,
            request.CompletedAtUtc,
            request.FailureReason);
    }
}
