import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { useFieldArray, useForm } from "react-hook-form";
import { useParams } from "react-router-dom";
import { z } from "zod";
import { useAuth } from "../../auth/context/AuthProvider";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";
import {
  ConstraintSeverity,
  constraintSeverityLabels,
  ConstraintType,
  constraintTypeLabels
} from "../../../shared/types/domain";
import { useWorkspaceSnapshot } from "../../projects/hooks/useWorkspaceSnapshot";
import { useRequirementWizardStore } from "../../requirements/store/requirementWizardStore";
import { requirementsApi } from "../../../shared/api/services/requirements.api";

const schema = z.object({
  constraints: z
    .array(
      z.object({
        title: z.string().min(3),
        description: z.string().min(8),
        type: z.number(),
        severity: z.number()
      })
    )
    .min(1)
});

type FormValues = z.infer<typeof schema>;

export function ConstraintEntryPage() {
  const { projectId = "" } = useParams();
  const queryClient = useQueryClient();
  const { session } = useAuth();
  const { requirements } = useWorkspaceSnapshot(projectId);
  const draft = useRequirementWizardStore((state) => state.draft);
  const hydrate = useRequirementWizardStore((state) => state.hydrate);
  const update = useRequirementWizardStore((state) => state.update);
  const { control, register, handleSubmit, reset } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      constraints: draft.constraints
    }
  });

  const fields = useFieldArray({
    control,
    name: "constraints"
  });

  useEffect(() => {
    hydrate(projectId, requirements.data);
  }, [hydrate, projectId, requirements.data]);

  useEffect(() => {
    reset({ constraints: draft.constraints });
  }, [draft.constraints, reset]);

  const saveMutation = useMutation({
    mutationFn: (values: FormValues) =>
      requirementsApi.save(session!.accessToken, projectId, {
        name: draft.name,
        summary: draft.summary,
        requirements: draft.requirements,
        constraints: values.constraints
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["workspace-requirements", projectId] });
    }
  });

  async function submit(values: FormValues) {
    update({ constraints: values.constraints });
    await saveMutation.mutateAsync(values);
  }

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Constraint capture"
        title="Record delivery and platform constraints"
        description="Describe the technical, business, regulatory, or hosting constraints that shape recommended design outputs."
        actions={
          <button onClick={handleSubmit(submit)} disabled={saveMutation.isPending}>
            {saveMutation.isPending ? "Saving..." : "Save constraints"}
          </button>
        }
      />

      <Panel
        title="Constraint statements"
        subtitle="These entries are persisted together with the current requirement baseline."
        actions={
          <button
            className="ghost-button"
            type="button"
            onClick={() =>
              fields.append({
                title: "",
                description: "",
                type: 2,
                severity: 2
              })
            }
          >
            Add constraint
          </button>
        }
      >
        <form className="stack" onSubmit={handleSubmit(submit)}>
          {fields.fields.map((field, index) => (
            <div key={field.id} className="item-card">
              <label>
                Title
                <input {...register(`constraints.${index}.title`)} />
              </label>
              <label>
                Description
                <textarea {...register(`constraints.${index}.description`)} />
              </label>
              <div className="form-grid">
                <label>
                  Type
                  <select {...register(`constraints.${index}.type`, { valueAsNumber: true })}>
                    {Object.values(ConstraintType)
                      .filter((value) => typeof value === "number")
                      .map((value) => (
                        <option key={value} value={value}>
                          {constraintTypeLabels[value as number]}
                        </option>
                      ))}
                  </select>
                </label>
                <label>
                  Severity
                  <select {...register(`constraints.${index}.severity`, { valueAsNumber: true })}>
                    {Object.values(ConstraintSeverity)
                      .filter((value) => typeof value === "number")
                      .map((value) => (
                        <option key={value} value={value}>
                          {constraintSeverityLabels[value as number]}
                        </option>
                      ))}
                  </select>
                </label>
              </div>
            </div>
          ))}
        </form>
      </Panel>
    </div>
  );
}
