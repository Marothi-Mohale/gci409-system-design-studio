import { apiRequest } from "../client";
import type { ArtifactSummary, ArtifactVersion, ExportResponse } from "../../types/api";

export const artifactsApi = {
  list(token: string, projectId: string) {
    return apiRequest<ArtifactSummary[]>(`/api/projects/${projectId}/artifacts`, { token });
  },
  versions(token: string, projectId: string, artifactId: string) {
    return apiRequest<ArtifactVersion[]>(`/api/projects/${projectId}/artifacts/${artifactId}/versions`, { token });
  },
  export(token: string, artifactVersionId: string, format: number) {
    return apiRequest<ExportResponse>(`/api/artifact-versions/${artifactVersionId}/exports`, {
      method: "POST",
      token,
      body: { format }
    });
  }
};
