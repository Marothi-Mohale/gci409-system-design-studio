import { apiRequest } from "../client";
import type { GenerationRequest } from "../../types/api";

export const generationApi = {
  list(token: string, projectId: string) {
    return apiRequest<GenerationRequest[]>(`/api/projects/${projectId}/generation-requests`, { token });
  },
  queue(token: string, projectId: string, artifactKinds: number[]) {
    return apiRequest<GenerationRequest>(`/api/projects/${projectId}/generation-requests`, {
      method: "POST",
      token,
      body: { artifactKinds, preferredFormat: 1 }
    });
  }
};
