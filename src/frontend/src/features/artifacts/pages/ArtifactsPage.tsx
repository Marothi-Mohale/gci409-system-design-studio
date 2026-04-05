import { Link, useParams } from "react-router-dom";
import { useWorkspaceSnapshot } from "../../projects/hooks/useWorkspaceSnapshot";
import { artifactKindLabels, artifactStatusLabels, ArtifactStatus } from "../../../shared/types/domain";
import { EmptyState } from "../../../shared/ui/EmptyState";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";
import { StatusBadge } from "../../../shared/ui/StatusBadge";

export function ArtifactsPage() {
  const { projectId = "" } = useParams();
  const snapshot = useWorkspaceSnapshot(projectId);

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Generated artifacts"
        title="Review the design output library"
        description="Browse versioned UML and supporting artifacts created for this workspace, then drill into a specific artifact for preview and export."
      />

      <Panel title="Artifact catalog" subtitle="Each artifact keeps its own status and current version.">
        {snapshot.artifacts.data?.length ? (
          <div className="artifact-grid">
            {snapshot.artifacts.data.map((artifact) => (
              <Link key={artifact.id} to={`/app/projects/${projectId}/artifacts/${artifact.id}`} className="item-card">
                <div className="panel-header">
                  <div>
                    <strong>{artifact.title}</strong>
                    <p className="subtle-text">{artifactKindLabels[artifact.artifactKind] ?? "Artifact"}</p>
                  </div>
                  <StatusBadge label={artifactStatusLabels[artifact.status] ?? "Draft"} tone={getArtifactTone(artifact.status)} />
                </div>
                <span className="subtle-text">Current version v{artifact.currentVersionNumber}</span>
                <span className="subtle-text">Created {new Date(artifact.createdAtUtc).toLocaleString()}</span>
              </Link>
            ))}
          </div>
        ) : (
          <EmptyState
            title="No artifacts have been generated"
            description="Run recommendations and queue generation to populate the workspace with UML and technical design outputs."
          />
        )}
      </Panel>
    </div>
  );
}

function getArtifactTone(status: number) {
  switch (status) {
    case ArtifactStatus.Approved:
      return "success" as const;
    case ArtifactStatus.Reviewed:
      return "warning" as const;
    default:
      return "neutral" as const;
  }
}
