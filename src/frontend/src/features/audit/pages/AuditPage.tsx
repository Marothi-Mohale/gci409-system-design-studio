import { useQueries } from "@tanstack/react-query";
import { useDeferredValue, useMemo, useState } from "react";
import { useAuth } from "../../auth/context/AuthProvider";
import { useProjects } from "../../projects/hooks/useProjects";
import { artifactsApi } from "../../../shared/api/services/artifacts.api";
import { generationApi } from "../../../shared/api/services/generation.api";
import { recommendationsApi } from "../../../shared/api/services/recommendations.api";
import { requirementsApi } from "../../../shared/api/services/requirements.api";
import type { ArtifactSummary, GenerationRequest, RecommendationSet, RequirementSetVersion } from "../../../shared/types/api";
import { artifactKindLabels, generationStatusLabels } from "../../../shared/types/domain";
import { EmptyState } from "../../../shared/ui/EmptyState";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";

type AuditEntry = {
  occurredAtUtc: string;
  projectName: string;
  title: string;
  detail: string;
};

export function AuditPage() {
  const { session } = useAuth();
  const { data: projects } = useProjects();
  const [search, setSearch] = useState("");
  const deferredSearch = useDeferredValue(search);

  const requirementQueries = useQueries({
    queries: (projects ?? []).map((project) => ({
      queryKey: ["audit-requirements", project.id],
      queryFn: () => requirementsApi.getCurrent(session!.accessToken, project.id),
      enabled: Boolean(session?.accessToken)
    }))
  });

  const recommendationQueries = useQueries({
    queries: (projects ?? []).map((project) => ({
      queryKey: ["audit-recommendations", project.id],
      queryFn: () => recommendationsApi.latest(session!.accessToken, project.id),
      enabled: Boolean(session?.accessToken)
    }))
  });

  const generationQueries = useQueries({
    queries: (projects ?? []).map((project) => ({
      queryKey: ["audit-generation", project.id],
      queryFn: () => generationApi.list(session!.accessToken, project.id),
      enabled: Boolean(session?.accessToken)
    }))
  });

  const artifactQueries = useQueries({
    queries: (projects ?? []).map((project) => ({
      queryKey: ["audit-artifacts", project.id],
      queryFn: () => artifactsApi.list(session!.accessToken, project.id),
      enabled: Boolean(session?.accessToken)
    }))
  });

  const activity = useMemo(() => {
    const entries: AuditEntry[] = [];

    (projects ?? []).forEach((project, index) => {
      const requirements = requirementQueries[index]?.data as RequirementSetVersion | undefined;
      const recommendations = recommendationQueries[index]?.data as RecommendationSet | undefined;
      const generationRequests = generationQueries[index]?.data as GenerationRequest[] | undefined;
      const artifacts = artifactQueries[index]?.data as ArtifactSummary[] | undefined;

      if (requirements) {
        entries.push({
          occurredAtUtc: requirements.createdAtUtc,
          projectName: project.name,
          title: "Requirement baseline saved",
          detail: `${requirements.requirements.length} requirements and ${requirements.constraints.length} constraints in version ${requirements.versionNumber}.`
        });
      }

      if (recommendations) {
        entries.push({
          occurredAtUtc: recommendations.createdAtUtc,
          projectName: project.name,
          title: "Recommendations generated",
          detail: `${recommendations.items.length} recommendations produced for the latest baseline.`
        });
      }

      (generationRequests ?? []).forEach((request) => {
        entries.push({
          occurredAtUtc: request.completedAtUtc ?? request.createdAtUtc,
          projectName: project.name,
          title: "Generation request activity",
          detail: `${generationStatusLabels[request.status] ?? "Status update"} for ${request.targets.length} target artifact(s).`
        });
      });

      (artifacts ?? []).forEach((artifact) => {
        entries.push({
          occurredAtUtc: artifact.createdAtUtc,
          projectName: project.name,
          title: "Artifact created",
          detail: `${artifactKindLabels[artifact.artifactKind] ?? "Artifact"} ${artifact.title} is available at version ${artifact.currentVersionNumber}.`
        });
      });
    });

    return entries.sort((left, right) => new Date(right.occurredAtUtc).getTime() - new Date(left.occurredAtUtc).getTime());
  }, [artifactQueries, generationQueries, projects, recommendationQueries, requirementQueries]);

  const filteredActivity = useMemo(() => {
    const term = deferredSearch.trim().toLowerCase();
    if (!term) {
      return activity;
    }

    return activity.filter(
      (entry) =>
        entry.projectName.toLowerCase().includes(term) ||
        entry.title.toLowerCase().includes(term) ||
        entry.detail.toLowerCase().includes(term)
    );
  }, [activity, deferredSearch]);

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Audit surface"
        title="Operational activity timeline"
        description="This page aggregates observable workspace activity until the dedicated append-only audit API is exposed end to end."
      />

      <Panel title="Filters" subtitle="Search across project names and activity summaries.">
        <label>
          Search timeline
          <input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search projects, actions, or details" />
        </label>
      </Panel>

      <Panel title="Timeline" subtitle="Most recent workspace activity first.">
        {filteredActivity.length ? (
          <div className="stack">
            {filteredActivity.map((entry, index) => (
              <div key={`${entry.projectName}-${entry.occurredAtUtc}-${index}`} className="item-card">
                <div className="panel-header">
                  <div>
                    <strong>{entry.title}</strong>
                    <p className="subtle-text">{entry.projectName}</p>
                  </div>
                  <span className="subtle-text">{new Date(entry.occurredAtUtc).toLocaleString()}</span>
                </div>
                <p className="subtle-text">{entry.detail}</p>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            title="No activity found"
            description="Create workspaces and start capturing requirements to populate the audit-oriented timeline."
          />
        )}
      </Panel>
    </div>
  );
}
