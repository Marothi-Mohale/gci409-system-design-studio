import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { useAuth } from "../../auth/context/AuthProvider";
import { useWorkspaceSnapshot } from "../../projects/hooks/useWorkspaceSnapshot";
import { generationApi } from "../../../shared/api/services/generation.api";
import { recommendationsApi } from "../../../shared/api/services/recommendations.api";
import { requirementsApi } from "../../../shared/api/services/requirements.api";
import { artifactKindLabels } from "../../../shared/types/domain";
import { EmptyState } from "../../../shared/ui/EmptyState";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";
import { StatusBadge } from "../../../shared/ui/StatusBadge";

export function RecommendationResultsPage() {
  const { projectId = "" } = useParams();
  const { session } = useAuth();
  const queryClient = useQueryClient();
  const snapshot = useWorkspaceSnapshot(projectId);
  const [selectedKinds, setSelectedKinds] = useState<number[]>([]);

  useEffect(() => {
    const recommendedKinds = snapshot.recommendations.data?.items.slice(0, 5).map((item) => item.artifactKind) ?? [];
    setSelectedKinds((current) => (current.length === 0 ? recommendedKinds : current));
  }, [snapshot.recommendations.data]);

  const recommendMutation = useMutation({
    mutationFn: () => recommendationsApi.generate(session!.accessToken, projectId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["workspace-recommendations", projectId] });
    }
  });

  const bootstrapMutation = useMutation({
    mutationFn: () => requirementsApi.bootstrap(session!.accessToken, projectId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["workspace-requirements", projectId] });
    }
  });

  const queueMutation = useMutation({
    mutationFn: (artifactKinds: number[]) => generationApi.queue(session!.accessToken, projectId, artifactKinds),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["workspace-generation", projectId] }),
        queryClient.invalidateQueries({ queryKey: ["workspace-artifacts", projectId] })
      ]);
    }
  });

  function toggleKind(artifactKind: number) {
    setSelectedKinds((current) =>
      current.includes(artifactKind) ? current.filter((value) => value !== artifactKind) : [...current, artifactKind]
    );
  }

  async function queueSelected() {
    if (selectedKinds.length === 0) {
      return;
    }

    await queueMutation.mutateAsync(selectedKinds);
  }

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Artifact recommendations"
        title="Analyze the current baseline"
        description="Generate explainable artifact recommendations from the latest requirements and constraints, then queue the ones we want to build."
        actions={
          <>
            <button onClick={() => recommendMutation.mutate()} disabled={!snapshot.requirements.data || recommendMutation.isPending}>
              {recommendMutation.isPending ? "Analyzing..." : "Run recommendation engine"}
            </button>
            <button
              className="ghost-button"
              onClick={queueSelected}
              disabled={selectedKinds.length === 0 || queueMutation.isPending}
            >
              {queueMutation.isPending ? "Queueing..." : "Queue selected artifacts"}
            </button>
          </>
        }
      />

      {!snapshot.requirements.data ? (
        <EmptyState
          title="No requirement baseline yet"
          description="Capture requirements and constraints first so the recommendation engine has evidence to evaluate."
          action={
            <div className="button-row">
              <button onClick={() => bootstrapMutation.mutate()} disabled={bootstrapMutation.isPending}>
                {bootstrapMutation.isPending ? "Building baseline..." : "Create baseline from brief"}
              </button>
            </div>
          }
        />
      ) : (
        <>
          <section className="stats-grid">
            <div className="stat-tile">
              <span className="subtle-text">Requirements</span>
              <strong>{snapshot.requirements.data.requirements.length}</strong>
            </div>
            <div className="stat-tile">
              <span className="subtle-text">Constraints</span>
              <strong>{snapshot.requirements.data.constraints.length}</strong>
            </div>
            <div className="stat-tile">
              <span className="subtle-text">Recommended artifacts</span>
              <strong>{snapshot.recommendations.data?.items.length ?? 0}</strong>
            </div>
          </section>

          <Panel
            title="Recommendation results"
            subtitle="Each recommendation includes a rationale and can be selected for asynchronous generation."
          >
            {snapshot.recommendations.data?.items.length ? (
              <div className="artifact-grid">
                {snapshot.recommendations.data.items.map((item) => {
                  const checked = selectedKinds.includes(item.artifactKind);
                  const confidence = Math.round(item.confidenceScore * 100);

                  return (
                    <label key={`${item.artifactKind}-${item.title}`} className="item-card">
                      <div className="panel-header">
                        <div>
                          <strong>{item.title}</strong>
                          <p className="subtle-text">{artifactKindLabels[item.artifactKind] ?? item.title}</p>
                        </div>
                        <input type="checkbox" checked={checked} onChange={() => toggleKind(item.artifactKind)} />
                      </div>
                      <div className="button-row">
                        <StatusBadge
                          label={`Confidence ${confidence}%`}
                          tone={confidence >= 75 ? "success" : confidence >= 50 ? "warning" : "danger"}
                        />
                      </div>
                      <p className="subtle-text">{item.rationale}</p>
                    </label>
                  );
                })}
              </div>
            ) : (
              <EmptyState
                title="No recommendations yet"
                description="Run the recommendation engine to determine the most relevant UML and system design outputs for this project."
              />
            )}
          </Panel>

          <Panel title="Generation handoff" subtitle="Track the latest queued work items created from recommendation results.">
            {snapshot.generationRequests.data?.length ? (
              <div className="stack">
                {snapshot.generationRequests.data.slice(0, 5).map((request) => (
                  <div key={request.id} className="item-card">
                    <strong>{new Date(request.createdAtUtc).toLocaleString()}</strong>
                    <span className="subtle-text">
                      {request.targets.length} target{request.targets.length === 1 ? "" : "s"} in queue
                    </span>
                    {request.failureReason && <span className="form-error">{request.failureReason}</span>}
                  </div>
                ))}
              </div>
            ) : (
              <EmptyState
                title="No generation requests yet"
                description="Select one or more recommendations and queue them to create versioned artifacts."
              />
            )}
          </Panel>
        </>
      )}
    </div>
  );
}
