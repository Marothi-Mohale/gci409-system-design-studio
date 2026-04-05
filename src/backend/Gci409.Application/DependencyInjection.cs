using Gci409.Application.Artifacts;
using Gci409.Application.Audit;
using Gci409.Application.Auth;
using Gci409.Application.Collaboration;
using Gci409.Application.Common;
using Gci409.Application.Exports;
using Gci409.Application.Generation;
using Gci409.Application.Profile;
using Gci409.Application.Projects;
using Gci409.Application.Recommendations;
using Gci409.Application.Requirements;
using Gci409.Application.Admin;
using Gci409.Application.Templates;
using Microsoft.Extensions.DependencyInjection;

namespace Gci409.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<AuthService>();
        services.AddScoped<ProfileService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<RequirementService>();
        services.AddScoped<RecommendationService>();
        services.AddScoped<GenerationService>();
        services.AddScoped<ArtifactService>();
        services.AddScoped<ExportService>();
        services.AddScoped<TemplateService>();
        services.AddScoped<CollaborationService>();
        services.AddScoped<AdminService>();
        services.AddScoped<AuditService>();
        return services;
    }
}
