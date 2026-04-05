using System.Text.RegularExpressions;
using Gci409.Application.Common;
using Gci409.Application.Requirements;
using Gci409.Domain.Requirements;

namespace Gci409.Infrastructure.Requirements;

public sealed partial class ProjectBriefRequirementBaselineBootstrapper : IRequirementBaselineBootstrapper
{
    public RequirementBaselineDraft BuildFromProjectBrief(string projectName, string? projectDescription)
    {
        var normalizedName = string.IsNullOrWhiteSpace(projectName) ? "Project" : projectName.Trim();
        var description = NormalizeWhitespace(projectDescription);
        var requirements = BuildRequirements(description);
        var constraints = BuildConstraints(description);

        if (requirements.Count == 0)
        {
            requirements.Add(CreateRequirement(
                1,
                "Establish the core solution workflow",
                $"The {normalizedName} solution must support the primary business workflow described in the workspace brief.",
                RequirementType.Functional,
                RequirementPriority.High));
        }

        var summary = !string.IsNullOrWhiteSpace(description)
            ? description
            : $"Initial architecture and design baseline for {normalizedName}.";

        return new RequirementBaselineDraft(
            Truncate($"{normalizedName} Baseline", 200),
            Truncate(summary, 4000),
            requirements,
            constraints);
    }

    private static List<RequirementInput> BuildRequirements(string description)
    {
        var requirements = new List<RequirementInput>();

        AddRequirementIf(
            requirements,
            ContainsAny(description, "application", "capture", "workflow", "origination", "submit"),
            "Manage the end-to-end business workflow",
            "The system must support end-to-end capture, processing, and lifecycle management of the primary business case from submission through completion.",
            RequirementType.Functional,
            RequirementPriority.Critical);

        AddRequirementIf(
            requirements,
            ContainsAny(description, "document", "upload", "attachment"),
            "Manage supporting documents",
            "The system must allow users to upload, validate, review, and retain supporting documents as part of the workflow.",
            RequirementType.Functional,
            RequirementPriority.High);

        AddRequirementIf(
            requirements,
            ContainsAny(description, "risk", "score", "scoring", "assessment"),
            "Support scoring and assessment decisions",
            "The system must support scoring, assessment, or evaluation steps that inform downstream approval and review decisions.",
            RequirementType.Functional,
            RequirementPriority.High);

        AddRequirementIf(
            requirements,
            ContainsAny(description, "approve", "approval", "reject", "status", "review"),
            "Drive approval and status transitions",
            "The system must support explicit review, approval, rejection, and status tracking transitions across the business process.",
            RequirementType.Functional,
            RequirementPriority.Critical);

        AddRequirementIf(
            requirements,
            ContainsAny(description, "report", "dashboard", "audit"),
            "Provide reporting and audit visibility",
            "The system must produce reports, dashboards, and audit-ready traceability for operational and governance stakeholders.",
            RequirementType.Reporting,
            RequirementPriority.High);

        var actorNames = ExtractActorNames(description);
        AddRequirementIf(
            requirements,
            actorNames.Count > 0 || ContainsAny(description, "user", "role", "agent", "manager", "analyst", "administrator"),
            "Support role-specific user experiences",
            actorNames.Count > 0
                ? $"The system must support secure role-specific experiences for {string.Join(", ", actorNames)}."
                : "The system must support secure role-specific experiences for the primary business and administrative users.",
            RequirementType.Functional,
            RequirementPriority.High);

        AddRequirementIf(
            requirements,
            ContainsAny(description, "security", "compliance", "popia", "rbac", "role-based"),
            "Protect sensitive business data",
            "The system must enforce strong security, access control, and compliance controls for sensitive business and customer data.",
            RequirementType.Security,
            RequirementPriority.Critical);

        AddRequirementIf(
            requirements,
            ContainsAny(description, "integration", "api", "external", "service"),
            "Support external integrations",
            "The system must expose or consume reliable service integrations where external systems or capabilities participate in the workflow.",
            RequirementType.Integration,
            RequirementPriority.Medium);

        if (requirements.Count < 3)
        {
            foreach (var sentence in SentenceSplitter().Split(description)
                         .Select(x => NormalizeWhitespace(x))
                         .Where(x => x.Length >= 40)
                         .Take(3 - requirements.Count))
            {
                requirements.Add(CreateRequirement(
                    requirements.Count + 1,
                    Truncate(sentence, 80),
                    sentence,
                    RequirementType.Functional,
                    RequirementPriority.Medium));
            }
        }

        return requirements;
    }

    private static List<ConstraintInput> BuildConstraints(string description)
    {
        var constraints = new List<ConstraintInput>();

        AddConstraintIf(
            constraints,
            ContainsAny(description, "c#", ".net", "asp.net"),
            "Use a .NET backend stack",
            "The solution must use a C#/.NET backend implementation.",
            ConstraintType.Technical,
            ConstraintSeverity.Mandatory);

        AddConstraintIf(
            constraints,
            ContainsAny(description, "react", "typescript"),
            "Use a React frontend stack",
            "The solution must use a React and TypeScript frontend implementation.",
            ConstraintType.Platform,
            ConstraintSeverity.Mandatory);

        AddConstraintIf(
            constraints,
            ContainsAny(description, "postgres", "postgresql"),
            "Use PostgreSQL for persistence",
            "The solution must persist its transactional data in PostgreSQL.",
            ConstraintType.Platform,
            ConstraintSeverity.Mandatory);

        AddConstraintIf(
            constraints,
            ContainsAny(description, "docker", "container"),
            "Support containerized deployment",
            "The solution must support containerized deployment and environment packaging with Docker.",
            ConstraintType.Platform,
            ConstraintSeverity.Important);

        AddConstraintIf(
            constraints,
            ContainsAny(description, "popia", "gdpr", "compliance", "regulatory"),
            "Meet regulatory compliance obligations",
            "The solution must satisfy applicable regulatory and compliance obligations, including privacy controls where required.",
            ConstraintType.Regulatory,
            ConstraintSeverity.Mandatory);

        AddConstraintIf(
            constraints,
            ContainsAny(description, "role-based access control", "rbac", "least privilege"),
            "Enforce role-based access control",
            "The solution must enforce role-based access control and least-privilege access for protected operations.",
            ConstraintType.Technical,
            ConstraintSeverity.Mandatory);

        AddConstraintIf(
            constraints,
            ContainsAny(description, "cloud hosting", "cloud", "hosting"),
            "Deploy to approved hosting",
            "The solution must run on approved hosting infrastructure and align with the target deployment environment.",
            ConstraintType.Platform,
            ConstraintSeverity.Important);

        var timelineMatch = TimelinePattern().Match(description);
        if (timelineMatch.Success)
        {
            AddConstraintIf(
                constraints,
                true,
                "Deliver within the stated timeline",
                $"The initial release must align with the stated delivery timeline of {timelineMatch.Value.Trim()}.",
                ConstraintType.Timeline,
                ConstraintSeverity.Mandatory);
        }

        return constraints;
    }

    private static RequirementInput CreateRequirement(
        int sequence,
        string title,
        string description,
        RequirementType type,
        RequirementPriority priority)
    {
        return new RequirementInput(
            $"REQ-{sequence:000}",
            Truncate(title, 200),
            Truncate(description, 4000),
            type,
            priority);
    }

    private static void AddRequirementIf(
        List<RequirementInput> requirements,
        bool condition,
        string title,
        string description,
        RequirementType type,
        RequirementPriority priority)
    {
        if (!condition)
        {
            return;
        }

        requirements.Add(CreateRequirement(requirements.Count + 1, title, description, type, priority));
    }

    private static void AddConstraintIf(
        List<ConstraintInput> constraints,
        bool condition,
        string title,
        string description,
        ConstraintType type,
        ConstraintSeverity severity)
    {
        if (!condition || constraints.Any(x => x.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        constraints.Add(new ConstraintInput(
            Truncate(title, 200),
            Truncate(description, 4000),
            type,
            severity));
    }

    private static List<string> ExtractActorNames(string description)
    {
        var usersIncludeIndex = description.IndexOf("users include", StringComparison.OrdinalIgnoreCase);
        if (usersIncludeIndex < 0)
        {
            return [];
        }

        var slice = description[(usersIncludeIndex + "users include".Length)..];
        var stopIndex = slice.IndexOf('.');
        if (stopIndex >= 0)
        {
            slice = slice[..stopIndex];
        }

        return slice
            .Split([",", " and "], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => NormalizeWhitespace(x))
            .Where(x => x.Length > 2)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(6)
            .ToList();
    }

    private static string NormalizeWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).Trim();
    }

    private static bool ContainsAny(string description, params string[] terms)
    {
        return terms.Any(term => description.Contains(term, StringComparison.OrdinalIgnoreCase));
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    [GeneratedRegex(@"(?<timeline>\b\d+\s*-\s*week\b|\b\d+\s*week\b|\b\d+\s*weeks\b)", RegexOptions.IgnoreCase)]
    private static partial Regex TimelinePattern();

    [GeneratedRegex(@"[.!?]+")]
    private static partial Regex SentenceSplitter();
}
