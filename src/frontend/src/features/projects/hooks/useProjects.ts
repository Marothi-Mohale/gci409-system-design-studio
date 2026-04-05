import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../../auth/context/AuthProvider";
import { projectsApi } from "../../../shared/api/services/projects.api";

export function useProjects() {
  const { session } = useAuth();

  return useQuery({
    queryKey: ["projects", session?.userId],
    queryFn: () => projectsApi.list(session!.accessToken),
    enabled: Boolean(session?.accessToken)
  });
}

export function useProjectDetail(projectId: string) {
  const { session } = useAuth();

  return useQuery({
    queryKey: ["project-detail", projectId],
    queryFn: () => projectsApi.get(session!.accessToken, projectId),
    enabled: Boolean(session?.accessToken && projectId)
  });
}
