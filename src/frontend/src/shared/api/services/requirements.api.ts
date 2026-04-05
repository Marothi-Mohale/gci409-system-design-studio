import { apiRequest } from "../client";
import type { RequirementSetVersion } from "../../types/api";

export const requirementsApi = {
  getCurrent(token: string, projectId: string) {
    return apiRequest<RequirementSetVersion>(`/api/projects/${projectId}/requirements/current`, { token });
  },
  save(
    token: string,
    projectId: string,
    body: {
      name: string;
      summary: string;
      requirements: RequirementSetVersion["requirements"];
      constraints: RequirementSetVersion["constraints"];
    }
  ) {
    return apiRequest<RequirementSetVersion>(`/api/projects/${projectId}/requirements`, { method: "POST", token, body });
  }
};
