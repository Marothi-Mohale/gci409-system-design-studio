import { useParams } from "react-router-dom";
import { useWorkspaceSnapshot } from "../hooks/useWorkspaceSnapshot";
import { generationStatusLabels } from "../../../shared/types/domain";
import { EmptyState } from "../../../shared/ui/EmptyState";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";
import { StatusBadge } from "../../../shared/ui/StatusBadge";

export function ProjectWorkspacePage() {
  const { projectId = "" } = useParams();
  const snapshot = useWorkspaceSnapshot(projectId);

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Workspace overview"
        title="Solution delivery snapshot"
        description="Review the current design maturity of this workspace before drilling into requirements, recommendations, or generated outputs."
      />

      <section className="stats-grid">
        <div className="stat-tile">
          <span className="subtle-text">Requirement versions</span>
          <strong>{snapshot.requirements.data?.versionNumber ?? 0}</strong>
        </div>
        <div className="stat-tile">
          <span className="subtle-text">Recommendations</span>
          <strong>{snapshot.recommendations.data?.items.length ?? 0}</strong>
        </div>
        <div className="stat-tile">
          <span className="subtle-text">Artifacts</span>
          <strong>{snapshot.artifacts.data?.length ?? 0}</strong>
        </div>
      </section>

      <div className="two-column">
        <Panel title="Requirement baseline" subtitle="The latest saved requirement set for this workspace.">
          {snapshot.requirements.data ? (
            <div className="stack">
              <strong>{snapshot.requirements.data.name}</strong>
              <p className="subtle-text">{snapshot.requirements.data.summary}</p>
              <span className="subtle-text">
                {snapshot.requirements.data.requirements.length} requirements | {snapshot.requirements.data.constraints.length} constraints
              </span>
            </div>
          ) : (
            <EmptyState title="No requirement baseline" description="Capture requirements to establish the first design baseline." />
          )}
        </Panel>

        <Panel title="Latest recommendations" subtitle="Recommended artifacts driven by the current baseline.">
          {snapshot.recommendations.data?.items.length ? (
            <div className="stack">
              {snapshot.recommendations.data.items.slice(0, 4).map((item) => (
                <div key={`${item.artifactKind}-${item.title}`} className="item-card">
                  <strong>{item.title}</strong>
                  <span className="subtle-text">Confidence {(item.confidenceScore * 100).toFixed(0)}%</span>
                  <p className="subtle-text">{item.rationale}</p>
                </div>
              ))}
            </div>
          ) : (
            <EmptyState title="No recommendations yet" description="Run the recommendation engine to see which design artifacts matter most." />
          )}
        </Panel>
      </div>

      <Panel title="Generation activity" subtitle="Recent asynchronous generation requests for this workspace.">
        {snapshot.generationRequests.data?.length ? (
          <div className="stack">
            {snapshot.generationRequests.data.map((request) => (
              <div key={request.id} className="item-card">
                <div className="panel-header">
                  <strong>{new Date(request.createdAtUtc).toLocaleString()}</strong>
                  <StatusBadge
                    label={generationStatusLabels[request.status] ?? `Status ${request.status}`}
                    tone={request.status === 3 ? "success" : request.status === 4 ? "danger" : "warning"}
                  />
                </div>
                <span className="subtle-text">{request.targets.length} selected artifact targets</span>
                {request.failureReason && <p className="form-error">{request.failureReason}</p>}
              </div>
            ))}
          </div>
        ) : (
          <EmptyState title="No generation activity" description="Queue a generation request once requirements and recommendations are ready." />
        )}
      </Panel>
    </div>
  );
}
