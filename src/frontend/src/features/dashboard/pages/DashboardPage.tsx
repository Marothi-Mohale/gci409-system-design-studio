import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { useNavigate } from "react-router-dom";
import { z } from "zod";
import { useAuth } from "../../auth/context/AuthProvider";
import { useProjects } from "../../projects/hooks/useProjects";
import { projectsApi } from "../../../shared/api/services/projects.api";
import { projectRoleLabels } from "../../../shared/types/domain";
import { EmptyState } from "../../../shared/ui/EmptyState";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";

const schema = z.object({
  name: z.string().min(3, "Project names should be at least 3 characters."),
  description: z.string().min(12, "Describe the solution initiative in a little more detail.")
});

type FormValues = z.infer<typeof schema>;

export function DashboardPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { session } = useAuth();
  const { data: projects } = useProjects();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: "",
      description: ""
    }
  });

  const createProject = useMutation({
    mutationFn: (values: FormValues) => projectsApi.create(session!.accessToken, values),
    onSuccess: async (project) => {
      reset();
      await queryClient.invalidateQueries({ queryKey: ["projects", session?.userId] });
      navigate(`/app/projects/${project.id}`);
    }
  });

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Portfolio dashboard"
        title="Active workspaces"
        description="Create or resume software design workspaces, then move into requirements, recommendations, artifacts, and exports."
      />

      <section className="stats-grid">
        <div className="stat-tile">
          <span className="subtle-text">Projects</span>
          <strong>{projects?.length ?? 0}</strong>
        </div>
        <div className="stat-tile">
          <span className="subtle-text">Auth session</span>
          <strong>{session?.fullName ?? "Guest"}</strong>
        </div>
        <div className="stat-tile">
          <span className="subtle-text">Workbench focus</span>
          <strong>Requirements to design</strong>
        </div>
      </section>

      <div className="two-column">
        <Panel title="Create project workspace" subtitle="Open a new design initiative and start capturing requirements.">
          <form className="stack" onSubmit={handleSubmit((values) => createProject.mutate(values))}>
            <label>
              Project name
              <input {...register("name")} />
              {errors.name && <span className="form-error">{errors.name.message}</span>}
            </label>
            <label>
              Description
              <textarea {...register("description")} />
              {errors.description && <span className="form-error">{errors.description.message}</span>}
            </label>
            <button type="submit" disabled={createProject.isPending}>
              {createProject.isPending ? "Creating..." : "Create workspace"}
            </button>
          </form>
        </Panel>

        <Panel title="Recent projects" subtitle="Jump directly into an active workspace.">
          <div className="stack">
            {projects?.length ? (
              projects.map((project) => (
                <button key={project.id} className="item-card" type="button" onClick={() => navigate(`/app/projects/${project.id}`)}>
                  <strong>{project.name}</strong>
                  <span className="subtle-text">
                    {project.key} | {projectRoleLabels[project.role] ?? `Role ${project.role}`}
                  </span>
                </button>
              ))
            ) : (
              <EmptyState
                title="No projects yet"
                description="Create your first workspace to begin capturing enterprise requirements and generating artifacts."
              />
            )}
          </div>
        </Panel>
      </div>
    </div>
  );
}
