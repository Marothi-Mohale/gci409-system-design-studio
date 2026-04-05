using Gci409.Application.Auth;
using Gci409.Application.Requirements;
using Gci409.Application.Templates;
using Gci409.Domain.Artifacts;
using Gci409.Domain.Requirements;

namespace Gci409.ApplicationTests;

public sealed class ValidationTests
{
    [Fact]
    public void RegisterUserRequestValidator_ShouldRejectWeakPasswords()
    {
        var validator = new RegisterUserRequestValidator();
        var result = validator.Validate(new RegisterUserRequest("Test User", "user@example.com", "weakpass"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }

    [Fact]
    public void SaveRequirementSetRequestValidator_ShouldRejectDuplicateRequirementCodes()
    {
        var validator = new SaveRequirementSetRequestValidator();
        var request = new SaveRequirementSetRequest(
            "Claims Requirements",
            "Claims handling baseline",
            [
                new RequirementInput("REQ-001", "Capture claim", "Capture a new claim.", RequirementType.Functional, RequirementPriority.High),
                new RequirementInput("REQ-001", "Approve claim", "Approve an existing claim.", RequirementType.Functional, RequirementPriority.High)
            ],
            []);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage.Contains("unique", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void CreateTemplateRequestValidator_ShouldRequireArtifactKinds()
    {
        var validator = new CreateTemplateRequestValidator();
        var request = new CreateTemplateRequest("Architecture Baseline", "Template for architecture outputs.", [], "# Template");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "ArtifactKinds");
    }
}
