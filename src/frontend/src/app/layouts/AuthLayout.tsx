import { Outlet } from "react-router-dom";

export function AuthLayout() {
  return (
    <div className="auth-shell">
      <aside className="brand-panel">
        <span className="eyebrow">Enterprise Design Workbench</span>
        <h1>gci409</h1>
        <p>
          Capture project intent, analyze requirements, and generate versioned UML diagrams, architecture views, and
          technical outputs.
        </p>
        <div className="feature-list">
          <span>Requirement workspaces</span>
          <span>Artifact recommendations</span>
          <span>UML and export previews</span>
          <span>Audit-ready delivery flow</span>
        </div>
      </aside>

      <main className="auth-main">
        <Outlet />
      </main>
    </div>
  );
}
