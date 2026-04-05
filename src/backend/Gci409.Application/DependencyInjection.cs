using Gci409.Application.Artifacts;
using Gci409.Application.Auth;
using Gci409.Application.Common;
using Gci409.Application.Generation;
using Gci409.Application.Projects;
using Gci409.Application.Recommendations;
using Gci409.Application.Requirements;
using Microsoft.Extensions.DependencyInjection;

namespace Gci409.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<AuthService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<RequirementService>();
        services.AddScoped<RecommendationService>();
        services.AddScoped<GenerationService>();
        services.AddScoped<ArtifactService>();
        return services;
    }
}
