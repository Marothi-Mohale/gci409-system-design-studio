import { Navigate, useLocation } from "react-router-dom";
import { PropsWithChildren } from "react";
import { useAuth } from "../context/AuthProvider";

export function RequireAuth({ children }: PropsWithChildren) {
  const { session } = useAuth();
  const location = useLocation();

  if (!session) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  return <>{children}</>;
}
