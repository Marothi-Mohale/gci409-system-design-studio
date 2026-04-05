import { NavLink, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "../../features/auth/context/AuthProvider";
import { ProjectRail } from "../../features/projects/components/ProjectRail";
import { useProjects } from "../../features/projects/hooks/useProjects";
import { ProjectRole, roleAllows } from "../../shared/types/domain";

export function AppLayout() {
  const { session, signOut } = useAuth();
  const location = useLocation();
  const { data: projects } = useProjects();
  const canSeeAdmin = (projects ?? []).some((project) => roleAllows(project.role, ProjectRole.Owner));
  const canSeeAudit = (projects ?? []).some((project) => roleAllows(project.role, ProjectRole.Reviewer));

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <span className="eyebrow">gci409</span>
          <strong>Design operations console</strong>
        </div>

        <nav className="sidebar-nav" aria-label="Global navigation">
          <NavLink to="/app/dashboard" className={({ isActive }) => navClass(isActive)}>
            Dashboard
          </NavLink>
          {canSeeAdmin && (
            <NavLink to="/app/admin" className={({ isActive }) => navClass(isActive)}>
              Admin
            </NavLink>
          )}
          {canSeeAudit && (
            <NavLink to="/app/admin/audit" className={({ isActive }) => navClass(isActive)}>
              Audit
            </NavLink>
          )}
        </nav>

        <ProjectRail currentPath={location.pathname} />
      </aside>

      <div className="shell-main">
        <header className="topbar">
          <div>
            <span className="eyebrow">Authenticated operator</span>
            <strong>{session?.fullName}</strong>
          </div>
          <div className="topbar-actions">
            <span className="subtle-text">{session?.email}</span>
            <button className="ghost-button" onClick={signOut}>
              Sign out
            </button>
          </div>
        </header>

        <main className="page-shell">
          <Outlet />
        </main>
      </div>
    </div>
  );
}

function navClass(isActive: boolean) {
  return isActive ? "nav-link nav-link--active" : "nav-link";
}
