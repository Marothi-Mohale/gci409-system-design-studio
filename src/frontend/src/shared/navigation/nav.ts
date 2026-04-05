import { ProjectRole, roleAllows } from "../types/domain";

type ProjectNavItem = {
  label: string;
  to: string;
  end?: boolean;
  minRole: ProjectRole;
};

export function getProjectNavigationItems(projectId: string, projectRole: number | undefined) {
  const items: ProjectNavItem[] = [
    { label: "Workspace", to: `/app/projects/${projectId}`, end: true, minRole: ProjectRole.Viewer },
    { label: "Requirements", to: `/app/projects/${projectId}/requirements`, minRole: ProjectRole.Contributor },
    { label: "Constraints", to: `/app/projects/${projectId}/constraints`, minRole: ProjectRole.Contributor },
    { label: "Recommendations", to: `/app/projects/${projectId}/recommendations`, minRole: ProjectRole.Viewer },
    { label: "Artifacts", to: `/app/projects/${projectId}/artifacts`, minRole: ProjectRole.Viewer },
    { label: "Exports", to: `/app/projects/${projectId}/exports`, minRole: ProjectRole.Viewer }
  ];

  return items.filter((item) => roleAllows(projectRole, item.minRole));
}
