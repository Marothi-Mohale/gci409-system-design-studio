import { startTransition } from "react";
import { useNavigate } from "react-router-dom";
import { useProjects } from "../hooks/useProjects";
import { projectRoleLabels } from "../../../shared/types/domain";
import { LoadingBlock } from "../../../shared/ui/LoadingBlock";

export function ProjectRail({ currentPath }: { currentPath: string }) {
  const navigate = useNavigate();
  const { data: projects, isLoading } = useProjects();

  if (isLoading) {
    return <LoadingBlock label="Loading projects" />;
  }

  return (
    <section className="project-rail" aria-label="Project workspaces">
      <span className="eyebrow">Projects</span>
      {projects?.map((project) => {
        const isActive = currentPath.includes(project.id);

        return (
          <button
            key={project.id}
            className={isActive ? "project-link project-link--active" : "project-link"}
            type="button"
            onClick={() =>
              startTransition(() => {
                navigate(`/app/projects/${project.id}`);
              })
            }
          >
            <strong>{project.name}</strong>
            <span className="project-link__meta">
              {project.key} | {projectRoleLabels[project.role] ?? `Role ${project.role}`}
            </span>
          </button>
        );
      })}
    </section>
  );
}
