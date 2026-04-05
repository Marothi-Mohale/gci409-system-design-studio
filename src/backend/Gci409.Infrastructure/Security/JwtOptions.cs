namespace Gci409.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "gci409-api";

    public string Audience { get; set; } = "gci409-client";

    public string SigningKey { get; set; } = "change-this-development-signing-key-at-deployment-time";

    public int ExpiryMinutes { get; set; } = 60;
}
