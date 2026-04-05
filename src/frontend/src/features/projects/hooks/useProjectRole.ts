import { useProjects } from "./useProjects";

export function useProjectRole(projectId: string) {
  const { data } = useProjects();
  return data?.find((project) => project.id === projectId)?.role;
}
