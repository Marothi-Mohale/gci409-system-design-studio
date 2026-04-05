using Gci409.Application.Common;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Audit;
using Gci409.Domain.Collaboration;
using Gci409.Domain.Generation;
using Gci409.Domain.Identity;
using Gci409.Domain.Projects;
using Gci409.Domain.Recommendations;
using Gci409.Domain.Requirements;
using Gci409.Domain.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Gci409.Infrastructure.Persistence;

public sealed class Gci409DbContext(DbContextOptions<Gci409DbContext> options) : DbContext(options), IGci409DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<PlatformRoleAssignment> PlatformRoleAssignments => Set<PlatformRoleAssignment>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMembership> ProjectMemberships => Set<ProjectMembership>();
    public DbSet<RequirementSet> RequirementSets => Set<RequirementSet>();
    public DbSet<RequirementSetVersion> RequirementSetVersions => Set<RequirementSetVersion>();
    public DbSet<RecommendationSet> RecommendationSets => Set<RecommendationSet>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<GenerationRequest> GenerationRequests => Set<GenerationRequest>();
    public DbSet<GenerationRequestTarget> GenerationRequestTargets => Set<GenerationRequestTarget>();
    public DbSet<GeneratedArtifact> GeneratedArtifacts => Set<GeneratedArtifact>();
    public DbSet<ArtifactVersion> ArtifactVersions => Set<ArtifactVersion>();
    public DbSet<ArtifactExport> ArtifactExports => Set<ArtifactExport>();
    public DbSet<UmlArtifactProfile> UmlArtifactProfiles => Set<UmlArtifactProfile>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<TemplateVersion> TemplateVersions => Set<TemplateVersion>();
    public DbSet<GenerationRule> GenerationRules => Set<GenerationRule>();
    public DbSet<GenerationRuleVersion> GenerationRuleVersions => Set<GenerationRuleVersion>();
    public DbSet<CommentThread> CommentThreads => Set<CommentThread>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureIdentity(modelBuilder);
        ConfigureProjects(modelBuilder);
        ConfigureRequirements(modelBuilder);
        ConfigureRecommendations(modelBuilder);
        ConfigureGeneration(modelBuilder);
        ConfigureArtifacts(modelBuilder);
        ConfigureTemplates(modelBuilder);
        ConfigureCollaboration(modelBuilder);
        ConfigureAudit(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    private static void ConfigureIdentity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users", "iam");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.Status);
            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
            builder.Property(x => x.PasswordHash).HasMaxLength(4000).IsRequired();
            builder.HasMany(x => x.RefreshTokens).WithOne().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.PlatformRoleAssignments).WithOne().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("refresh_tokens", "iam");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => x.TokenHash).IsUnique();
            builder.HasIndex(x => new { x.UserId, x.ExpiresAtUtc, x.RevokedAtUtc });
            builder.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
            builder.Property(x => x.ExpiresAtUtc).HasColumnType("timestamp with time zone");
            builder.Property(x => x.RevokedAtUtc).HasColumnType("timestamp with time zone");
            builder.HasOne<User>().WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("roles", "iam");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.Name, x.Scope }).IsUnique();
            builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            builder.HasMany(x => x.Permissions).WithOne().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Permission>(builder =>
        {
            builder.ToTable("permissions", "iam");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => x.Code).IsUnique();
            builder.Property(x => x.Code).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<RolePermission>(builder =>
        {
            builder.ToTable("role_permissions", "iam");
            builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
            builder.HasIndex(x => x.PermissionId);
            builder.HasOne<Role>().WithMany(x => x.Permissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Permission>().WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlatformRoleAssignment>(builder =>
        {
            builder.ToTable("platform_role_assignments", "iam");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
            builder.HasIndex(x => x.RoleId);
            builder.HasOne<User>().WithMany(x => x.PlatformRoleAssignments).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Role>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureProjects(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(builder =>
        {
            builder.ToTable("projects", "projects");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => x.Key).IsUnique();
            builder.HasIndex(x => new { x.OwnerUserId, x.Status });
            builder.Property(x => x.Key).HasMaxLength(32).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4000);
            builder.HasMany(x => x.Memberships).WithOne().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<User>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProjectMembership>(builder =>
        {
            builder.ToTable("project_memberships", "projects");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
            builder.HasIndex(x => new { x.UserId, x.Status });
            builder.HasOne<Project>().WithMany(x => x.Memberships).HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<User>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRequirements(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RequirementSet>(builder =>
        {
            builder.ToTable("requirement_sets", "requirements");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => x.ProjectId).IsUnique();
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Overview).HasMaxLength(4000).IsRequired();
            builder.HasMany(x => x.Versions).WithOne(x => x.RequirementSet).HasForeignKey(x => x.RequirementSetId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequirementSetVersion>(builder =>
        {
            builder.ToTable("requirement_set_versions", "requirements");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.RequirementSetId, x.VersionNumber }).IsUnique();
            builder.HasIndex(x => x.CreatedAtUtc);
            builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
            builder.HasMany(x => x.Requirements).WithOne().HasForeignKey(x => x.RequirementSetVersionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Constraints).WithOne().HasForeignKey(x => x.RequirementSetVersionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequirementItem>(builder =>
        {
            builder.ToTable("requirement_items", "requirements");
            builder.HasIndex(x => new { x.RequirementSetVersionId, x.Code }).IsUnique();
            builder.HasIndex(x => new { x.RequirementSetVersionId, x.Type, x.Priority });
            builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        });

        modelBuilder.Entity<ConstraintItem>(builder =>
        {
            builder.ToTable("constraint_items", "requirements");
            builder.HasIndex(x => new { x.RequirementSetVersionId, x.Type, x.Severity });
            builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        });
    }

    private static void ConfigureRecommendations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RecommendationSet>(builder =>
        {
            builder.ToTable("recommendation_sets", "generation");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.ProjectId, x.CreatedAtUtc });
            builder.HasIndex(x => x.RequirementSetVersionId);
            builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.RecommendationSetId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<RequirementSetVersion>().WithMany().HasForeignKey(x => x.RequirementSetVersionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Recommendation>(builder =>
        {
            builder.ToTable("recommendations", "generation");
            builder.HasIndex(x => new { x.RecommendationSetId, x.ArtifactKind }).IsUnique();
            builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Rationale).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.ConfidenceScore).HasPrecision(5, 4);
        });
    }

    private static void ConfigureGeneration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenerationRequest>(builder =>
        {
            builder.ToTable("generation_requests", "generation");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.ProjectId, x.Status, x.CreatedAtUtc });
            builder.HasIndex(x => x.RequirementSetVersionId);
            builder.Property(x => x.StartedAtUtc).HasColumnType("timestamp with time zone");
            builder.Property(x => x.CompletedAtUtc).HasColumnType("timestamp with time zone");
            builder.Property(x => x.FailureReason).HasMaxLength(4000);
            builder.HasMany(x => x.Targets).WithOne().HasForeignKey(x => x.GenerationRequestId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<RequirementSetVersion>().WithMany().HasForeignKey(x => x.RequirementSetVersionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GenerationRequestTarget>(builder =>
        {
            builder.ToTable("generation_request_targets", "generation");
            builder.HasIndex(x => new { x.GenerationRequestId, x.ArtifactKind }).IsUnique();
            builder.HasOne<GenerationRequest>().WithMany(x => x.Targets).HasForeignKey(x => x.GenerationRequestId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureArtifacts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeneratedArtifact>(builder =>
        {
            builder.ToTable("generated_artifacts", "artifacts");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.ProjectId, x.Status });
            builder.HasIndex(x => new { x.ProjectId, x.ArtifactKind, x.CreatedAtUtc });
            builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
            builder.HasMany(x => x.Versions).WithOne(x => x.GeneratedArtifact).HasForeignKey(x => x.GeneratedArtifactId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.UmlProfile).WithOne().HasForeignKey<UmlArtifactProfile>(x => x.GeneratedArtifactId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UmlArtifactProfile>(builder =>
        {
            builder.ToTable("uml_profiles", "artifacts");
            builder.HasIndex(x => x.DiagramType);
            builder.HasOne<GeneratedArtifact>().WithOne(x => x.UmlProfile).HasForeignKey<UmlArtifactProfile>(x => x.GeneratedArtifactId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ArtifactVersion>(builder =>
        {
            builder.ToTable("artifact_versions", "artifacts");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.GeneratedArtifactId, x.VersionNumber }).IsUnique();
            builder.HasIndex(x => x.GenerationRequestId);
            builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.Content).HasColumnType("text").IsRequired();
            builder.Property(x => x.RepresentationsJson).HasColumnName("representations_jsonb").HasColumnType("jsonb");
            builder.HasMany(x => x.Exports).WithOne().HasForeignKey(x => x.ArtifactVersionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<GeneratedArtifact>().WithMany(x => x.Versions).HasForeignKey(x => x.GeneratedArtifactId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<GenerationRequest>().WithMany().HasForeignKey(x => x.GenerationRequestId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArtifactExport>(builder =>
        {
            builder.ToTable("artifact_exports", "artifacts");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.ArtifactVersionId, x.Format, x.CreatedAtUtc });
            builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Content).HasColumnType("text").IsRequired();
            builder.HasOne<ArtifactVersion>().WithMany(x => x.Exports).HasForeignKey(x => x.ArtifactVersionId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureTemplates(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Template>(builder =>
        {
            builder.ToTable("templates", "generation");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.ProjectId, x.Name });
            builder.HasIndex(x => x.Status);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            builder.HasMany(x => x.Versions).WithOne().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TemplateVersion>(builder =>
        {
            builder.ToTable("template_versions", "generation");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.TemplateId, x.VersionNumber }).IsUnique();
            builder.Property(x => x.Content).HasColumnType("text").IsRequired();
            builder.Property(x => x.SupportedArtifactKindsCsv).HasColumnType("text").IsRequired();
        });

        modelBuilder.Entity<GenerationRule>(builder =>
        {
            builder.ToTable("generation_rules", "generation");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.ProjectId, x.Name, x.Scope });
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            builder.HasMany(x => x.Versions).WithOne().HasForeignKey(x => x.GenerationRuleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GenerationRuleVersion>(builder =>
        {
            builder.ToTable("generation_rule_versions", "generation");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.GenerationRuleId, x.VersionNumber }).IsUnique();
            builder.Property(x => x.RuleDefinitionJson).HasColumnName("rule_definition_jsonb").HasColumnType("jsonb").IsRequired();
        });
    }

    private static void ConfigureCollaboration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CommentThread>(builder =>
        {
            builder.ToTable("comment_threads", "collaboration");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.ProjectId, x.TargetType, x.TargetId }).IsUnique();
            builder.HasIndex(x => new { x.ProjectId, x.Status });
            builder.HasMany(x => x.Comments).WithOne().HasForeignKey(x => x.CommentThreadId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(builder =>
        {
            builder.ToTable("comments", "collaboration");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.CommentThreadId, x.CreatedAtUtc });
            builder.Property(x => x.Body).HasColumnType("text").IsRequired();
            builder.HasOne<CommentThread>().WithMany(x => x.Comments).HasForeignKey(x => x.CommentThreadId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAudit(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("audit_logs", "audit");
            ConfigureAuditableEntity(builder);
            builder.HasIndex(x => new { x.ProjectId, x.CreatedAtUtc });
            builder.HasIndex(x => new { x.ActorUserId, x.CreatedAtUtc });
            builder.HasIndex(x => new { x.EntityType, x.EntityId, x.CreatedAtUtc });
            builder.HasIndex(x => new { x.Action, x.CreatedAtUtc });
            builder.Property(x => x.Action).HasMaxLength(128).IsRequired();
            builder.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
            builder.Property(x => x.EntityId).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.CorrelationId).HasMaxLength(128);
            builder.Property(x => x.MetadataJson).HasColumnName("metadata_jsonb").HasColumnType("jsonb");
            builder.HasOne<User>().WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Project>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureAuditableEntity<TEntity>(EntityTypeBuilder<TEntity> builder)
        where TEntity : Gci409.Domain.Common.AuditableEntity
    {
        builder.Property(x => x.CreatedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.LastModifiedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.LastModifiedByUserId);
    }
}
