using Gci409.Domain.Artifacts;
using Gci409.Domain.Audit;
using Gci409.Domain.Collaboration;
using Gci409.Domain.Generation;
using Gci409.Domain.Identity;
using Gci409.Domain.Projects;
using Gci409.Domain.Recommendations;
using Gci409.Domain.Requirements;
using Gci409.Domain.Templates;
using Gci409.Application.Requirements;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Common;

public interface IGci409DbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<PlatformRoleAssignment> PlatformRoleAssignments { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectMembership> ProjectMemberships { get; }
    DbSet<RequirementSet> RequirementSets { get; }
    DbSet<RequirementSetVersion> RequirementSetVersions { get; }
    DbSet<RecommendationSet> RecommendationSets { get; }
    DbSet<Recommendation> Recommendations { get; }
    DbSet<GenerationRequest> GenerationRequests { get; }
    DbSet<GenerationRequestTarget> GenerationRequestTargets { get; }
    DbSet<GeneratedArtifact> GeneratedArtifacts { get; }
    DbSet<ArtifactVersion> ArtifactVersions { get; }
    DbSet<ArtifactExport> ArtifactExports { get; }
    DbSet<UmlArtifactProfile> UmlArtifactProfiles { get; }
    DbSet<Template> Templates { get; }
    DbSet<TemplateVersion> TemplateVersions { get; }
    DbSet<GenerationRule> GenerationRules { get; }
    DbSet<GenerationRuleVersion> GenerationRuleVersions { get; }
    DbSet<CommentThread> CommentThreads { get; }
    DbSet<Comment> Comments { get; }
    DbSet<AuditLog> AuditLogs { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string password, string passwordHash);
}

public interface IJwtTokenService
{
    JwtTokenResult CreateTokens(User user, IReadOnlyCollection<string> platformRoles);
}

public interface IAuditWriter
{
    Task WriteAsync(Guid? actorUserId, Guid? projectId, string action, string entityType, string entityId, string description, string? metadataJson = null, CancellationToken cancellationToken = default);
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface IRefreshTokenProtector
{
    string Hash(string refreshToken);
}

public interface ICorrelationContextAccessor
{
    string? CorrelationId { get; set; }
}

public interface IArtifactRecommendationEngine
{
    Task<IReadOnlyCollection<ArtifactRecommendationDraft>> RecommendAsync(RecommendationInput input, CancellationToken cancellationToken = default);
}

public interface IRequirementBaselineBootstrapper
{
    RequirementBaselineDraft BuildFromProjectBrief(string projectName, string? projectDescription);
}

public interface IArtifactGenerationEngine
{
    Task<IReadOnlyCollection<ArtifactDraft>> GenerateAsync(ArtifactGenerationInput input, CancellationToken cancellationToken = default);
}

public interface IArtifactExportContentResolver
{
    string ResolveContent(ArtifactVersion version, OutputFormat format);
}

public interface IArtifactPdfRenderer
{
    byte[] Render(ArtifactPdfRenderRequest request);
}

public sealed record JwtTokenResult(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAtUtc);

public sealed record ArtifactRecommendationDraft(ArtifactKind ArtifactKind, string Title, string Rationale, decimal ConfidenceScore, RecommendationStrength Strength);

public sealed record RecommendationInput(string ProjectName, string RequirementSummary, IReadOnlyCollection<string> RequirementDescriptions, IReadOnlyCollection<string> ConstraintDescriptions);

public sealed record RequirementBaselineDraft(
    string Name,
    string Summary,
    IReadOnlyCollection<RequirementInput> Requirements,
    IReadOnlyCollection<ConstraintInput> Constraints);

public sealed record ArtifactDraft(
    ArtifactKind ArtifactKind,
    string Title,
    string Summary,
    OutputFormat PrimaryFormat,
    string Content,
    string? RepresentationsJson,
    UmlDiagramType DiagramType);

public sealed record ArtifactGenerationInput(
    string ProjectName,
    string RequirementSummary,
    IReadOnlyCollection<string> RequirementDescriptions,
    IReadOnlyCollection<string> ConstraintDescriptions,
    IReadOnlyCollection<ArtifactKind> ArtifactKinds);

public sealed record ArtifactPdfRenderRequest(
    string Title,
    ArtifactKind ArtifactKind,
    int VersionNumber,
    string Summary,
    OutputFormat SourceFormat,
    string SourceContent,
    DateTimeOffset CreatedAtUtc);
