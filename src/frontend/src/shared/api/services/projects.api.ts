import { apiRequest } from "../client";
import type { ProjectDetail, ProjectSummary } from "../../types/api";

export const projectsApi = {
  list(token: string) {
    return apiRequest<ProjectSummary[]>("/api/projects", { token });
  },
  create(token: string, body: { name: string; description: string }) {
    return apiRequest<ProjectSummary>("/api/projects", { method: "POST", token, body });
  },
  get(token: string, projectId: string) {
    return apiRequest<ProjectDetail>(`/api/projects/${projectId}`, { token });
  }
};
