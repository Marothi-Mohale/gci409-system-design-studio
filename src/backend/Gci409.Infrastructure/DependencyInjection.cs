using Gci409.Application.Common;
using Gci409.Infrastructure.Exports;
using Gci409.Infrastructure.Logging;
using Gci409.Infrastructure.Generation;
using Gci409.Infrastructure.OpenAi;
using Gci409.Infrastructure.Persistence;
using Gci409.Infrastructure.Recommendations;
using Gci409.Infrastructure.Requirements;
using Gci409.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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

        services
            .AddOptions<OpenAiOptions>()
            .Bind(configuration.GetSection(OpenAiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContext<Gci409DbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddHttpClient<IOpenAiJsonClient, OpenAiJsonClient>((serviceProvider, client) =>
        {
            var openAiOptions = serviceProvider.GetRequiredService<IOptions<OpenAiOptions>>().Value;
            client.BaseAddress = new Uri(openAiOptions.BaseUrl, UriKind.Absolute);
            client.Timeout = TimeSpan.FromSeconds(openAiOptions.RequestTimeoutSeconds);
        });

        services.AddScoped<IGci409DbContext>(provider => provider.GetRequiredService<Gci409DbContext>());
        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IRefreshTokenProtector, RefreshTokenProtector>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddSingleton<IRequirementBaselineBootstrapper, ProjectBriefRequirementBaselineBootstrapper>();
        services.AddScoped<IArtifactExportContentResolver, ArtifactExportContentResolver>();
        services.AddSingleton<IArtifactPdfRenderer, ArtifactPdfRenderer>();
        services.AddSingleton<RuleBasedArtifactRecommendationEngine>();
        services.AddSingleton<OpenAiArtifactRecommendationEngine>();
        services.AddSingleton<IArtifactRecommendationEngine, HybridArtifactRecommendationEngine>();
        services.AddSingleton<RuleBasedArtifactGenerationEngine>();
        services.AddSingleton<OpenAiArtifactGenerationEngine>();
        services.AddSingleton<IArtifactGenerationEngine, HybridArtifactGenerationEngine>();

        return services;
    }
}
