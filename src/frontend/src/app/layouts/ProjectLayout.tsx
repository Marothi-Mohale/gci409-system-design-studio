import { NavLink, Outlet, useParams } from "react-router-dom";
import { useProjectDetail } from "../../features/projects/hooks/useProjects";
import { EmptyState } from "../../shared/ui/EmptyState";
import { LoadingBlock } from "../../shared/ui/LoadingBlock";
import { PageHeader } from "../../shared/ui/PageHeader";
import { getProjectNavigationItems } from "../../shared/navigation/nav";
import { useProjectRole } from "../../features/projects/hooks/useProjectRole";

export function ProjectLayout() {
  const { projectId = "" } = useParams();
  const { data: project, isLoading } = useProjectDetail(projectId);
  const projectRole = useProjectRole(projectId);

  if (isLoading) {
    return <LoadingBlock label="Loading workspace" />;
  }

  if (!project) {
    return <EmptyState title="Workspace not found" description="Select a project from the rail to continue." />;
  }

  const navItems = getProjectNavigationItems(projectId, projectRole);

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Project workspace"
        title={project.name}
        description={project.description || "Manage requirements, recommendations, generated artifacts, and exports."}
      />

      <nav className="tab-nav" aria-label="Project navigation">
        {navItems.map((item) => (
          <NavLink key={item.to} end={item.end} to={item.to} className={({ isActive }) => (isActive ? "tab-link tab-link--active" : "tab-link")}>
            {item.label}
          </NavLink>
        ))}
      </nav>

      <Outlet />
    </div>
  );
}
