import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useNavigate, useParams } from "react-router-dom";
import { useAuth } from "../../auth/context/AuthProvider";
import { generationApi } from "../../../shared/api/services/generation.api";
import { recommendationsApi } from "../../../shared/api/services/recommendations.api";
import { requirementsApi } from "../../../shared/api/services/requirements.api";
import { artifactKindLabels, generationStatusLabels } from "../../../shared/types/domain";
import { EmptyState } from "../../../shared/ui/EmptyState";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";
import { StatusBadge } from "../../../shared/ui/StatusBadge";
import { useWorkspaceSnapshot } from "../hooks/useWorkspaceSnapshot";

export function ProjectWorkspacePage() {
  const { projectId = "" } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { session } = useAuth();
  const snapshot = useWorkspaceSnapshot(projectId);
  const hasRequirements = Boolean(snapshot.requirements.data);
  const hasRecommendations = Boolean(snapshot.recommendations.data?.items.length);
  const hasArtifacts = Boolean(snapshot.artifacts.data?.length);
  const hasProjectBrief = Boolean(snapshot.project.data?.description?.trim());

  async function refreshWorkspace() {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ["workspace-requirements", projectId] }),
      queryClient.invalidateQueries({ queryKey: ["workspace-recommendations", projectId] }),
      queryClient.invalidateQueries({ queryKey: ["workspace-generation", projectId] }),
      queryClient.invalidateQueries({ queryKey: ["workspace-artifacts", projectId] })
    ]);
  }

  const bootstrapMutation = useMutation({
    mutationFn: () => requirementsApi.bootstrap(session!.accessToken, projectId),
    onSuccess: refreshWorkspace
  });

  const recommendMutation = useMutation({
    mutationFn: () => recommendationsApi.generate(session!.accessToken, projectId),
    onSuccess: refreshWorkspace
  });

  const starterPackMutation = useMutation({
    mutationFn: async () => {
      const requirementBaseline = snapshot.requirements.data ?? (await requirementsApi.bootstrap(session!.accessToken, projectId));
      const currentRecommendations =
        snapshot.recommendations.data?.requirementSetVersionId === requirementBaseline.versionId
          ? snapshot.recommendations.data
          : undefined;
      const recommendationSet = currentRecommendations ?? (await recommendationsApi.generate(session!.accessToken, projectId));
      const artifactKinds = recommendationSet.items.slice(0, 5).map((item) => item.artifactKind);

      if (artifactKinds.length === 0) {
        throw new Error("No artifacts were recommended for the current workspace brief.");
      }

      return generationApi.queue(session!.accessToken, projectId, artifactKinds);
    },
    onSuccess: refreshWorkspace
  });

  const actionError =
    (starterPackMutation.error as Error | null) ??
    (recommendMutation.error as Error | null) ??
    (bootstrapMutation.error as Error | null) ??
    null;

  const recommendedArtifactKinds = snapshot.recommendations.data?.items.slice(0, 5).map((item) => item.artifactKind) ?? [];

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Workspace overview"
        title="Solution delivery snapshot"
        description="Move from the workspace brief into a saved baseline, explainable recommendations, and generated design outputs."
        actions={
          <>
            {!hasArtifacts && (
              <button onClick={() => starterPackMutation.mutate()} disabled={starterPackMutation.isPending}>
                {starterPackMutation.isPending ? "Building design pack..." : "Build starter design pack"}
              </button>
            )}
            <button className="ghost-button" onClick={() => navigate(`/app/projects/${projectId}/requirements`)}>
              Refine requirements
            </button>
            {hasArtifacts && (
              <button className="ghost-button" onClick={() => navigate(`/app/projects/${projectId}/artifacts`)}>
                Open artifacts
              </button>
            )}
          </>
        }
      />

      {actionError && (
        <div className="message" role="alert">
          {actionError.message}
        </div>
      )}

      {(bootstrapMutation.isSuccess || recommendMutation.isSuccess || starterPackMutation.isSuccess) && (
        <div className="message" role="status">
          {starterPackMutation.isSuccess
            ? "Starter design pack queued. Generated UML and supporting artifacts will appear in the workspace as the worker completes them."
            : recommendMutation.isSuccess
              ? "Artifact recommendations were refreshed from the latest baseline."
              : "An initial requirement baseline was created from the workspace brief."}
        </div>
      )}

      <Panel
        title="Get to your first design pack"
        subtitle="Use the workspace brief you already entered, or refine the baseline manually before generation."
        actions={
          <div className="button-row">
            {!hasRequirements && (
              <button className="ghost-button" onClick={() => bootstrapMutation.mutate()} disabled={bootstrapMutation.isPending}>
                {bootstrapMutation.isPending ? "Building baseline..." : "Create baseline from brief"}
              </button>
            )}
            {hasRequirements && !hasRecommendations && (
              <button className="ghost-button" onClick={() => recommendMutation.mutate()} disabled={recommendMutation.isPending}>
                {recommendMutation.isPending ? "Analyzing..." : "Run recommendation engine"}
              </button>
            )}
            <button className="ghost-button" onClick={() => navigate(`/app/projects/${projectId}/constraints`)}>
              Edit constraints
            </button>
          </div>
        }
      >
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

        <div className="stack">
          <div className="item-card">
            <strong>1. Establish a baseline</strong>
            <p className="subtle-text">
              {hasRequirements
                ? "A saved baseline exists and can drive recommendations and generation."
                : hasProjectBrief
                  ? "Use the workspace brief to create an initial requirement baseline automatically."
                  : "Open the requirements wizard to capture the baseline manually."}
            </p>
          </div>
          <div className="item-card">
            <strong>2. Review recommendations</strong>
            <p className="subtle-text">
              {hasRecommendations
                ? `The current baseline recommends ${snapshot.recommendations.data?.items.length ?? 0} design outputs.`
                : "Run the recommendation engine to determine which UML diagrams, DFDs, ERDs, and summaries matter most."}
            </p>
          </div>
          <div className="item-card">
            <strong>3. Queue the starter design pack</strong>
            <p className="subtle-text">
              Build a first-pass set of UML and supporting artifacts, then drill into the artifact library for preview and export.
            </p>
          </div>
        </div>
      </Panel>

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
            <EmptyState
              title="No requirement baseline"
              description="Create a baseline from the workspace brief or open the requirement wizard to capture it manually."
              action={
                <div className="button-row">
                  <button onClick={() => bootstrapMutation.mutate()} disabled={bootstrapMutation.isPending}>
                    {bootstrapMutation.isPending ? "Building baseline..." : "Create from brief"}
                  </button>
                  <button className="ghost-button" onClick={() => navigate(`/app/projects/${projectId}/requirements`)}>
                    Capture manually
                  </button>
                </div>
              }
            />
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
            <EmptyState
              title="No recommendations yet"
              description="Generate recommendations to identify which UML diagrams, DFDs, ERDs, and technical outputs fit this workspace."
              action={
                <button onClick={() => recommendMutation.mutate()} disabled={!hasRequirements || recommendMutation.isPending}>
                  {recommendMutation.isPending ? "Analyzing..." : "Run recommendation engine"}
                </button>
              }
            />
          )}
        </Panel>
      </div>

      <Panel title="Starter design pack" subtitle="These are the next artifacts that will be generated from the current baseline.">
        {recommendedArtifactKinds.length ? (
          <div className="artifact-grid">
            {recommendedArtifactKinds.map((artifactKind) => (
              <div key={artifactKind} className="item-card">
                <strong>{artifactKindLabels[artifactKind] ?? `Artifact ${artifactKind}`}</strong>
                <p className="subtle-text">Included in the next starter generation batch.</p>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            title="No starter design pack yet"
            description="Once recommendations are available, the workspace can queue a first-pass design pack covering UML and supporting outputs."
            action={
              <button onClick={() => starterPackMutation.mutate()} disabled={starterPackMutation.isPending}>
                {starterPackMutation.isPending ? "Building design pack..." : "Build starter design pack"}
              </button>
            }
          />
        )}
      </Panel>

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
          <EmptyState
            title="No generation activity"
            description="Queue a starter design pack to generate UML diagrams, DFDs, ERDs, and related design outputs."
            action={
              <button onClick={() => starterPackMutation.mutate()} disabled={starterPackMutation.isPending}>
                {starterPackMutation.isPending ? "Building design pack..." : "Build starter design pack"}
              </button>
            }
          />
        )}
      </Panel>
    </div>
  );
}
