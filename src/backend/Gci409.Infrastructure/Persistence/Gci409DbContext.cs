using Gci409.Application.Common;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Audit;
using Gci409.Domain.Generation;
using Gci409.Domain.Identity;
using Gci409.Domain.Projects;
using Gci409.Domain.Recommendations;
using Gci409.Domain.Requirements;
using Gci409.Domain.Templates;
using Microsoft.EntityFrameworkCore;

namespace Gci409.Infrastructure.Persistence;

public sealed class Gci409DbContext(DbContextOptions<Gci409DbContext> options) : DbContext(options), IGci409DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
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
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(builder =>
        {
            builder.ToTable("users", "iam");
            builder.HasIndex(x => x.Email).IsUnique();
            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Email).HasMaxLength(320).IsRequired();
            builder.Property(x => x.PasswordHash).HasMaxLength(4000).IsRequired();
            builder.HasMany(x => x.RefreshTokens).WithOne().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.PlatformRoleAssignments).WithOne().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("refresh_tokens", "iam");
            builder.HasIndex(x => x.TokenHash).IsUnique();
        });

        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("roles", "iam");
            builder.HasIndex(x => new { x.Name, x.Scope }).IsUnique();
            builder.HasMany(x => x.Permissions).WithOne().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Permission>(builder =>
        {
            builder.ToTable("permissions", "iam");
            builder.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(builder =>
        {
            builder.ToTable("role_permissions", "iam");
            builder.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
        });

        modelBuilder.Entity<PlatformRoleAssignment>(builder =>
        {
            builder.ToTable("platform_role_assignments", "iam");
            builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
        });

        modelBuilder.Entity<Project>(builder =>
        {
            builder.ToTable("projects", "projects");
            builder.HasIndex(x => x.Key).IsUnique();
            builder.Property(x => x.Key).HasMaxLength(32).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4000);
            builder.HasMany(x => x.Memberships).WithOne().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectMembership>(builder =>
        {
            builder.ToTable("project_memberships", "projects");
            builder.HasIndex(x => new { x.ProjectId, x.UserId }).IsUnique();
        });

        modelBuilder.Entity<RequirementSet>(builder =>
        {
            builder.ToTable("requirement_sets", "requirements");
            builder.HasIndex(x => x.ProjectId).IsUnique();
            builder.HasMany(x => x.Versions).WithOne(x => x.RequirementSet).HasForeignKey(x => x.RequirementSetId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequirementSetVersion>(builder =>
        {
            builder.ToTable("requirement_set_versions", "requirements");
            builder.HasIndex(x => new { x.RequirementSetId, x.VersionNumber }).IsUnique();
            builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
            builder.HasMany(x => x.Requirements).WithOne().HasForeignKey(x => x.RequirementSetVersionId).OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(x => x.Constraints).WithOne().HasForeignKey(x => x.RequirementSetVersionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequirementItem>(builder =>
        {
            builder.ToTable("requirement_items", "requirements");
            builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
            builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        });

        modelBuilder.Entity<ConstraintItem>(builder =>
        {
            builder.ToTable("constraint_items", "requirements");
            builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
        });

        modelBuilder.Entity<RecommendationSet>(builder =>
        {
            builder.ToTable("recommendation_sets", "generation");
            builder.HasMany(x => x.Items).WithOne().HasForeignKey(x => x.RecommendationSetId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Recommendation>(builder =>
        {
            builder.ToTable("recommendations", "generation");
            builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Rationale).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.ConfidenceScore).HasPrecision(5, 2);
        });

        modelBuilder.Entity<GenerationRequest>(builder =>
        {
            builder.ToTable("generation_requests", "generation");
            builder.HasMany(x => x.Targets).WithOne().HasForeignKey(x => x.GenerationRequestId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GenerationRequestTarget>(builder =>
        {
            builder.ToTable("generation_request_targets", "generation");
        });

        modelBuilder.Entity<GeneratedArtifact>(builder =>
        {
            builder.ToTable("generated_artifacts", "artifacts");
            builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
            builder.HasMany(x => x.Versions).WithOne(x => x.GeneratedArtifact).HasForeignKey(x => x.GeneratedArtifactId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.UmlProfile).WithOne().HasForeignKey<UmlArtifactProfile>(x => x.GeneratedArtifactId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UmlArtifactProfile>(builder =>
        {
            builder.ToTable("uml_profiles", "artifacts");
        });

        modelBuilder.Entity<ArtifactVersion>(builder =>
        {
            builder.ToTable("artifact_versions", "artifacts");
            builder.HasIndex(x => new { x.GeneratedArtifactId, x.VersionNumber }).IsUnique();
            builder.Property(x => x.Summary).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.Content).HasColumnType("text").IsRequired();
            builder.Property(x => x.RepresentationsJson).HasColumnType("text");
            builder.HasMany(x => x.Exports).WithOne().HasForeignKey(x => x.ArtifactVersionId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ArtifactExport>(builder =>
        {
            builder.ToTable("artifact_exports", "artifacts");
            builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
            builder.Property(x => x.Content).HasColumnType("text").IsRequired();
        });

        modelBuilder.Entity<Template>(builder =>
        {
            builder.ToTable("templates", "generation");
            builder.HasMany(x => x.Versions).WithOne().HasForeignKey(x => x.TemplateId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TemplateVersion>(builder =>
        {
            builder.ToTable("template_versions", "generation");
            builder.Property(x => x.Content).HasColumnType("text").IsRequired();
        });

        modelBuilder.Entity<GenerationRule>(builder =>
        {
            builder.ToTable("generation_rules", "generation");
            builder.HasMany(x => x.Versions).WithOne().HasForeignKey(x => x.GenerationRuleId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GenerationRuleVersion>(builder =>
        {
            builder.ToTable("generation_rule_versions", "generation");
            builder.Property(x => x.RuleDefinitionJson).HasColumnType("text").IsRequired();
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("audit_logs", "audit");
            builder.Property(x => x.Action).HasMaxLength(128).IsRequired();
            builder.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
            builder.Property(x => x.EntityId).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(4000).IsRequired();
            builder.Property(x => x.MetadataJson).HasColumnType("text");
        });

        base.OnModelCreating(modelBuilder);
    }
}
