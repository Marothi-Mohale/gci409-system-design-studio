import { apiRequest } from "../client";
import type { RecommendationSet } from "../../types/api";

export const recommendationsApi = {
  latest(token: string, projectId: string) {
    return apiRequest<RecommendationSet>(`/api/projects/${projectId}/recommendations/latest`, { token });
  },
  generate(token: string, projectId: string) {
    return apiRequest<RecommendationSet>(`/api/projects/${projectId}/recommendations`, { method: "POST", token });
  }
};
