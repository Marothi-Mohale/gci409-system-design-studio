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
  name: z
    .string()
    .trim()
    .min(1, "Enter a project name.")
    .max(200, "Project names must be 200 characters or fewer."),
  description: z
    .string()
    .trim()
    .max(4000, "Descriptions must be 4000 characters or fewer.")
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
      queryClient.setQueryData(["project-detail", project.id], project);
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
          <form className="stack" onSubmit={handleSubmit(async (values) => createProject.mutateAsync(values))}>
            {createProject.isError && (
              <div className="message" role="alert">
                {createProject.error instanceof Error ? createProject.error.message : "Unable to create the workspace."}
              </div>
            )}
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
            <p className="subtle-text">Create the workspace now, then capture requirements and constraints inside it.</p>
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
