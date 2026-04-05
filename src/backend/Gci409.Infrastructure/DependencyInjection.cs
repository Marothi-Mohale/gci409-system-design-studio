using Gci409.Application.Common;
using Gci409.Infrastructure.Generation;
using Gci409.Infrastructure.Persistence;
using Gci409.Infrastructure.Recommendations;
using Gci409.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gci409.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddDbContext<Gci409DbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IGci409DbContext>(provider => provider.GetRequiredService<Gci409DbContext>());
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuditWriter, AuditWriter>();
        services.AddSingleton<IArtifactRecommendationEngine, RuleBasedArtifactRecommendationEngine>();
        services.AddSingleton<IArtifactGenerationEngine, RuleBasedArtifactGenerationEngine>();

        return services;
    }
}
