import { useAuth } from "../../auth/context/AuthProvider";
import { useProjects } from "../../projects/hooks/useProjects";
import { env } from "../../../shared/config/env";
import { ProjectRole, projectRoleLabels } from "../../../shared/types/domain";
import { EmptyState } from "../../../shared/ui/EmptyState";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";

export function AdminPage() {
  const { session } = useAuth();
  const { data: projects } = useProjects();
  const ownerProjects = projects?.filter((project) => project.role === ProjectRole.Owner) ?? [];

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Administration"
        title="Platform operating posture"
        description="Review the current frontend-to-backend environment, access context, and the enterprise capabilities already wired into the application shell."
      />

      {!ownerProjects.length ? (
        <EmptyState
          title="Limited admin scope"
          description="You are authenticated, but you do not currently own a workspace. Administrative controls can still be designed here while deeper platform APIs are added."
        />
      ) : null}

      <section className="stats-grid">
        <div className="stat-tile">
          <span className="subtle-text">Signed in as</span>
          <strong>{session?.fullName ?? "Unknown user"}</strong>
        </div>
        <div className="stat-tile">
          <span className="subtle-text">API endpoint</span>
          <strong>{env.apiBaseUrl}</strong>
        </div>
        <div className="stat-tile">
          <span className="subtle-text">Owned workspaces</span>
          <strong>{ownerProjects.length}</strong>
        </div>
      </section>

      <div className="two-column">
        <Panel title="Current administrative scope" subtitle="Accessible workspaces and role posture for this operator.">
          <div className="stack">
            {(projects ?? []).map((project) => (
              <div key={project.id} className="item-card">
                <strong>{project.name}</strong>
                <span className="subtle-text">
                  {project.key} | {projectRoleLabels[project.role] ?? `Role ${project.role}`}
                </span>
              </div>
            ))}
          </div>
        </Panel>

        <Panel title="Capability matrix" subtitle="Enterprise-ready controls already represented in the current product baseline.">
          <div className="stack">
            <div className="item-card">
              <strong>Identity and access</strong>
              <span className="subtle-text">JWT access, refresh flow, project roles, and route protection.</span>
            </div>
            <div className="item-card">
              <strong>Design operations</strong>
              <span className="subtle-text">Requirements, recommendations, artifact generation, previews, and exports.</span>
            </div>
            <div className="item-card">
              <strong>Governance</strong>
              <span className="subtle-text">Admin and audit surfaces are in place, with room for deeper policy management APIs.</span>
            </div>
          </div>
        </Panel>
      </div>
    </div>
  );
}
