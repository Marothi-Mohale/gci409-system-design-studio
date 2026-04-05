import { createContext, PropsWithChildren, useContext, useMemo, useState } from "react";
import type { AuthResponse } from "../../../shared/types/api";
import { authApi } from "../../../shared/api/services/auth.api";

type AuthContextValue = {
  session: AuthResponse | null;
  login: (payload: { email: string; password: string }) => Promise<AuthResponse>;
  register: (payload: { fullName: string; email: string; password: string }) => Promise<AuthResponse>;
  refresh: () => Promise<AuthResponse | null>;
  signOut: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);
const storageKey = "gci409.session";
let refreshPromise: Promise<AuthResponse | null> | null = null;

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthResponse | null>(() => {
    const stored = localStorage.getItem(storageKey);
    return stored ? (JSON.parse(stored) as AuthResponse) : null;
  });

  const value = useMemo<AuthContextValue>(
    () => ({
      session,
      async login(payload) {
        const nextSession = await authApi.login(payload);
        persistSession(nextSession, setSession);
        return nextSession;
      },
      async register(payload) {
        const nextSession = await authApi.register(payload);
        persistSession(nextSession, setSession);
        return nextSession;
      },
      async refresh() {
        const currentSession = session;
        if (!currentSession?.refreshToken) {
          return null;
        }

        if (refreshPromise) {
          return await refreshPromise;
        }

        refreshPromise = (async () => {
          try {
            const nextSession = await authApi.refresh({ refreshToken: currentSession.refreshToken });
            persistSession(nextSession, setSession);
            return nextSession;
          } catch {
            throw new Error("Unable to refresh authentication session.");
          } finally {
            refreshPromise = null;
          }
        })();

        return await refreshPromise;
      },
      signOut() {
        localStorage.removeItem(storageKey);
        setSession(null);
      }
    }),
    [session]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider.");
  }

  return context;
}

function persistSession(session: AuthResponse, setSession: (value: AuthResponse) => void) {
  localStorage.setItem(storageKey, JSON.stringify(session));
  setSession(session);
}
