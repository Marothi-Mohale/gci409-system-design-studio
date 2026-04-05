import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { AuthForm } from "../components/AuthForm";
import { useAuth } from "../context/AuthProvider";

export function RegisterPage() {
  const { register: registerUser } = useAuth();
  const navigate = useNavigate();
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(values: { fullName: string; email: string; password: string }) {
    try {
      setError(null);
      await registerUser(values);
      navigate("/app/dashboard", { replace: true });
    } catch (submissionError) {
      setError(submissionError instanceof Error ? submissionError.message : "Unable to register.");
    }
  }

  return (
    <div className="stack">
      <AuthForm
        mode="register"
        title="Create operator account"
        description="Register a new authenticated operator for enterprise design workbench access."
        submitLabel="Create account"
        onSubmit={handleSubmit}
        error={error}
      />
      <p className="subtle-text">
        Already have access? <Link to="/login">Sign in</Link>
      </p>
    </div>
  );
}
