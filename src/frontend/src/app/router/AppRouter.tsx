import { Navigate, Route, Routes } from "react-router-dom";
import { AuthLayout } from "../layouts/AuthLayout";
import { AppLayout } from "../layouts/AppLayout";
import { ProjectLayout } from "../layouts/ProjectLayout";
import { LoginPage } from "../../features/auth/pages/LoginPage";
import { RegisterPage } from "../../features/auth/pages/RegisterPage";
import { DashboardPage } from "../../features/dashboard/pages/DashboardPage";
import { ProjectWorkspacePage } from "../../features/projects/pages/ProjectWorkspacePage";
import { RequirementWizardPage } from "../../features/requirements/pages/RequirementWizardPage";
import { ConstraintEntryPage } from "../../features/constraints/pages/ConstraintEntryPage";
import { RecommendationResultsPage } from "../../features/recommendations/pages/RecommendationResultsPage";
import { ArtifactsPage } from "../../features/artifacts/pages/ArtifactsPage";
import { ArtifactDetailPage } from "../../features/artifacts/pages/ArtifactDetailPage";
import { ExportsPage } from "../../features/exports/pages/ExportsPage";
import { AdminPage } from "../../features/admin/pages/AdminPage";
import { AuditPage } from "../../features/audit/pages/AuditPage";
import { RequireAuth } from "../../features/auth/components/RequireAuth";

export function AppRouter() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>

      <Route
        path="/app"
        element={
          <RequireAuth>
            <AppLayout />
          </RequireAuth>
        }
      >
        <Route index element={<Navigate to="/app/dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />

        <Route path="projects/:projectId" element={<ProjectLayout />}>
          <Route index element={<ProjectWorkspacePage />} />
          <Route path="requirements" element={<RequirementWizardPage />} />
          <Route path="constraints" element={<ConstraintEntryPage />} />
          <Route path="recommendations" element={<RecommendationResultsPage />} />
          <Route path="artifacts" element={<ArtifactsPage />} />
          <Route path="artifacts/:artifactId" element={<ArtifactDetailPage />} />
          <Route path="exports" element={<ExportsPage />} />
        </Route>

        <Route path="admin" element={<AdminPage />} />
        <Route path="admin/audit" element={<AuditPage />} />
      </Route>

      <Route path="*" element={<Navigate to="/app/dashboard" replace />} />
    </Routes>
  );
}
