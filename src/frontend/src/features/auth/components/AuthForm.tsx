import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";

const registerSchema = z.object({
  fullName: z.string().min(2, "Enter the operator's full name."),
  email: z.string().email("Enter a valid email address."),
  password: z
    .string()
    .min(12, "Passwords must be at least 12 characters.")
    .regex(/[A-Z]/, "Passwords must include at least one uppercase letter.")
    .regex(/[a-z]/, "Passwords must include at least one lowercase letter.")
    .regex(/[0-9]/, "Passwords must include at least one number.")
});

const loginSchema = registerSchema.omit({ fullName: true });

type RegisterValues = z.infer<typeof registerSchema>;
type LoginValues = z.infer<typeof loginSchema>;

type AuthFormProps =
  | {
      mode: "register";
      title: string;
      description: string;
      submitLabel: string;
      onSubmit: (values: RegisterValues) => Promise<void>;
      error: string | null;
    }
  | {
      mode: "login";
      title: string;
      description: string;
      submitLabel: string;
      onSubmit: (values: LoginValues) => Promise<void>;
      error: string | null;
    };

export function AuthForm(props: AuthFormProps) {
  if (props.mode === "register") {
    return <RegisterAuthForm {...props} />;
  }

  return <LoginAuthForm {...props} />;
}

function RegisterAuthForm(props: Extract<AuthFormProps, { mode: "register" }>) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<RegisterValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { fullName: "", email: "", password: "" }
  });

  return (
    <form className="auth-card stack" onSubmit={handleSubmit(props.onSubmit)}>
      <div className="stack">
        <span className="eyebrow">Access</span>
        <h2>{props.title}</h2>
        <p className="subtle-text">{props.description}</p>
      </div>

      {props.error && <div className="message">{props.error}</div>}

      <label>
        Full name
        <input {...register("fullName")} autoComplete="name" />
        {errors.fullName && <span className="form-error">{errors.fullName.message}</span>}
      </label>

      <label>
        Email
        <input {...register("email")} autoComplete="email" />
        {errors.email && <span className="form-error">{errors.email.message}</span>}
      </label>

      <label>
        Password
        <input type="password" {...register("password")} autoComplete="new-password" />
        {errors.password && <span className="form-error">{errors.password.message}</span>}
      </label>
      <p className="subtle-text">Use at least 12 characters with uppercase, lowercase, and a number.</p>

      <button disabled={isSubmitting} type="submit">
        {isSubmitting ? "Submitting..." : props.submitLabel}
      </button>
    </form>
  );
}

function LoginAuthForm(props: Extract<AuthFormProps, { mode: "login" }>) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" }
  });

  return (
    <form className="auth-card stack" onSubmit={handleSubmit(props.onSubmit)}>
      <div className="stack">
        <span className="eyebrow">Access</span>
        <h2>{props.title}</h2>
        <p className="subtle-text">{props.description}</p>
      </div>

      {props.error && <div className="message">{props.error}</div>}

      <label>
        Email
        <input {...register("email")} autoComplete="email" />
        {errors.email && <span className="form-error">{errors.email.message}</span>}
      </label>

      <label>
        Password
        <input type="password" {...register("password")} autoComplete="current-password" />
        {errors.password && <span className="form-error">{errors.password.message}</span>}
      </label>

      <button disabled={isSubmitting} type="submit">
        {isSubmitting ? "Submitting..." : props.submitLabel}
      </button>
    </form>
  );
}
