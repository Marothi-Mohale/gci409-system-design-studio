using Gci409.Application.Common;
using Gci409.Infrastructure.Exports;
using Gci409.Infrastructure.Logging;
using Gci409.Infrastructure.Generation;
using Gci409.Infrastructure.Persistence;
using Gci409.Infrastructure.Recommendations;
using Gci409.Infrastructure.Requirements;
using Gci409.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gci409.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<Gci409DbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IGci409DbContext>(provider => provider.GetRequiredService<Gci409DbContext>());
        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IRefreshTokenProtector, RefreshTokenProtector>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddSingleton<IRequirementBaselineBootstrapper, ProjectBriefRequirementBaselineBootstrapper>();
        services.AddScoped<IArtifactExportContentResolver, ArtifactExportContentResolver>();
        services.AddSingleton<IArtifactPdfRenderer, ArtifactPdfRenderer>();
        services.AddSingleton<IArtifactRecommendationEngine, RuleBasedArtifactRecommendationEngine>();
        services.AddSingleton<IArtifactGenerationEngine, RuleBasedArtifactGenerationEngine>();

        return services;
    }
}
