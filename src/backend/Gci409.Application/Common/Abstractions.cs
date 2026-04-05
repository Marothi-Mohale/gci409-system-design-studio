using Gci409.Domain.Artifacts;
using Gci409.Domain.Audit;
using Gci409.Domain.Generation;
using Gci409.Domain.Identity;
using Gci409.Domain.Projects;
using Gci409.Domain.Recommendations;
using Gci409.Domain.Requirements;
using Gci409.Domain.Templates;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Application.Common;

public interface IGci409DbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
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
    JwtTokenResult CreateTokens(User user);
}

public interface IAuditWriter
{
    Task WriteAsync(Guid? actorUserId, Guid? projectId, string action, string entityType, string entityId, string description, string? metadataJson = null, CancellationToken cancellationToken = default);
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface IArtifactRecommendationEngine
{
    IReadOnlyCollection<ArtifactRecommendationDraft> Recommend(RecommendationInput input);
}

public interface IArtifactGenerationEngine
{
    IReadOnlyCollection<ArtifactDraft> Generate(ArtifactGenerationInput input);
}

public sealed record JwtTokenResult(string AccessToken, string RefreshToken, DateTimeOffset ExpiresAtUtc);

public sealed record ArtifactRecommendationDraft(ArtifactKind ArtifactKind, string Title, string Rationale, decimal ConfidenceScore, RecommendationStrength Strength);

public sealed record RecommendationInput(string ProjectName, string RequirementSummary, IReadOnlyCollection<string> RequirementDescriptions, IReadOnlyCollection<string> ConstraintDescriptions);

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
