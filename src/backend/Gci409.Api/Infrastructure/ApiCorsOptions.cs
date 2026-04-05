using System.ComponentModel.DataAnnotations;

namespace Gci409.Api.Infrastructure;

public sealed class ApiCorsOptions
{
    public const string SectionName = "Cors";

    [MinLength(1)]
    public string[] AllowedOrigins { get; set; } =
    [
        "http://127.0.0.1:5173",
        "http://localhost:5173"
    ];
}
