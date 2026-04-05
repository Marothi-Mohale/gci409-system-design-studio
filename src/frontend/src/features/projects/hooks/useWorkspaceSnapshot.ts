import { useQueries } from "@tanstack/react-query";
import { useAuth } from "../../auth/context/AuthProvider";
import { projectsApi } from "../../../shared/api/services/projects.api";
import { requirementsApi } from "../../../shared/api/services/requirements.api";
import { recommendationsApi } from "../../../shared/api/services/recommendations.api";
import { generationApi } from "../../../shared/api/services/generation.api";
import { artifactsApi } from "../../../shared/api/services/artifacts.api";

export function useWorkspaceSnapshot(projectId: string) {
  const { session } = useAuth();
  const token = session?.accessToken;

  const results = useQueries({
    queries: [
      {
        queryKey: ["workspace-project", projectId],
        queryFn: () => projectsApi.get(token!, projectId),
        enabled: Boolean(token && projectId)
      },
      {
        queryKey: ["workspace-requirements", projectId],
        queryFn: () => requirementsApi.getCurrent(token!, projectId),
        enabled: Boolean(token && projectId)
      },
      {
        queryKey: ["workspace-recommendations", projectId],
        queryFn: () => recommendationsApi.latest(token!, projectId),
        enabled: Boolean(token && projectId)
      },
      {
        queryKey: ["workspace-generation", projectId],
        queryFn: () => generationApi.list(token!, projectId),
        enabled: Boolean(token && projectId),
        refetchInterval: 3000
      },
      {
        queryKey: ["workspace-artifacts", projectId],
        queryFn: () => artifactsApi.list(token!, projectId),
        enabled: Boolean(token && projectId),
        refetchInterval: 3000
      }
    ]
  });

  return {
    project: results[0],
    requirements: results[1],
    recommendations: results[2],
    generationRequests: results[3],
    artifacts: results[4]
  };
}
