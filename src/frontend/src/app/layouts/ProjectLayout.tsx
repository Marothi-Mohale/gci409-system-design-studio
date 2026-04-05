import { NavLink, Outlet, useParams } from "react-router-dom";
import { useAuth } from "../../features/auth/context/AuthProvider";
import { useProjectDetail } from "../../features/projects/hooks/useProjects";
import { EmptyState } from "../../shared/ui/EmptyState";
import { LoadingBlock } from "../../shared/ui/LoadingBlock";
import { PageHeader } from "../../shared/ui/PageHeader";
import { getProjectNavigationItems } from "../../shared/navigation/nav";
import { ProjectRole } from "../../shared/types/domain";

export function ProjectLayout() {
  const { projectId = "" } = useParams();
  const { session } = useAuth();
  const { data: project, isLoading } = useProjectDetail(projectId);

  if (isLoading) {
    return <LoadingBlock label="Loading workspace" />;
  }

  if (!project) {
    return <EmptyState title="Workspace not found" description="Select a project from the rail to continue." />;
  }

  const projectRole = project.members?.find((member) => member.userId === session?.userId)?.role ?? ProjectRole.Viewer;
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
