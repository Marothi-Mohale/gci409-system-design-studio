using System.ComponentModel.DataAnnotations;

namespace Gci409.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = "gci409-api";

    [Required]
    public string Audience { get; set; } = "gci409-client";

    [Required]
    [MinLength(32)]
    public string SigningKey { get; set; } = "change-this-development-signing-key-at-deployment-time";

    [Range(5, 1440)]
    public int ExpiryMinutes { get; set; } = 60;
}
