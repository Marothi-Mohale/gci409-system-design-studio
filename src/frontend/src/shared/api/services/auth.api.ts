import { apiRequest } from "../client";
import type { AuthResponse } from "../../types/api";

export const authApi = {
  register(body: { fullName: string; email: string; password: string }) {
    return apiRequest<AuthResponse>("/api/auth/register", { method: "POST", body });
  },
  login(body: { email: string; password: string }) {
    return apiRequest<AuthResponse>("/api/auth/login", { method: "POST", body });
  },
  refresh(body: { refreshToken: string }) {
    return apiRequest<AuthResponse>("/api/auth/refresh", { method: "POST", body });
  }
};
