using Gci409.Domain.Projects;

namespace Gci409.Application.Projects;

public sealed record CreateProjectRequest(string Name, string Description);

public sealed record ProjectSummary(Guid Id, string Key, string Name, string Description, ProjectStatus Status, ProjectRole Role, DateTimeOffset CreatedAtUtc);

public sealed record ProjectDetail(Guid Id, string Key, string Name, string Description, ProjectStatus Status, IReadOnlyCollection<ProjectMemberSummary> Members);

public sealed record ProjectMemberSummary(Guid UserId, ProjectRole Role, MembershipStatus Status);
