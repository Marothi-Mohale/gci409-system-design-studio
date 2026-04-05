import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { AuthForm } from "../components/AuthForm";
import { useAuth } from "../context/AuthProvider";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(values: { email: string; password: string }) {
    try {
      setError(null);
      await login(values);
      navigate(location.state?.from ?? "/app/dashboard", { replace: true });
    } catch (submissionError) {
      setError(submissionError instanceof Error ? submissionError.message : "Unable to sign in.");
    }
  }

  return (
    <div className="stack">
      <AuthForm
        mode="login"
        title="Sign in"
        description="Access your active project workspaces, recommendations, and generated artifacts."
        submitLabel="Sign in"
        onSubmit={handleSubmit}
        error={error}
      />
      <p className="subtle-text">
        Need an operator account? <Link to="/register">Create one</Link>
      </p>
    </div>
  );
}
