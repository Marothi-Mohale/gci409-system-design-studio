import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { useEffect, useState } from "react";
import { useFieldArray, useForm } from "react-hook-form";
import { useParams } from "react-router-dom";
import { z } from "zod";
import { requirementsApi } from "../../../shared/api/services/requirements.api";
import {
  RequirementPriority,
  requirementPriorityLabels,
  RequirementType,
  requirementTypeLabels
} from "../../../shared/types/domain";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";
import { useAuth } from "../../auth/context/AuthProvider";
import { useWorkspaceSnapshot } from "../../projects/hooks/useWorkspaceSnapshot";
import { useRequirementWizardStore } from "../store/requirementWizardStore";

const schema = z.object({
  name: z.string().min(3),
  summary: z.string().min(20, "Provide a concise but meaningful summary."),
  requirements: z
    .array(
      z.object({
        code: z.string().min(3),
        title: z.string().min(3),
        description: z.string().min(10),
        type: z.number(),
        priority: z.number()
      })
    )
    .min(1)
});

type FormValues = z.infer<typeof schema>;

export function RequirementWizardPage() {
  const { projectId = "" } = useParams();
  const queryClient = useQueryClient();
  const { session } = useAuth();
  const [step, setStep] = useState(0);
  const { requirements } = useWorkspaceSnapshot(projectId);
  const draft = useRequirementWizardStore((state) => state.draft);
  const hydrate = useRequirementWizardStore((state) => state.hydrate);
  const update = useRequirementWizardStore((state) => state.update);
  const {
    control,
    register,
    handleSubmit,
    reset,
    getValues,
    formState: { errors }
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: draft.name,
      summary: draft.summary,
      requirements: draft.requirements
    }
  });

  const requirementFields = useFieldArray({
    control,
    name: "requirements"
  });

  useEffect(() => {
    hydrate(projectId, requirements.data);
  }, [hydrate, projectId, requirements.data]);

  useEffect(() => {
    reset({
      name: draft.name,
      summary: draft.summary,
      requirements: draft.requirements
    });
  }, [draft, reset]);

  const saveMutation = useMutation({
    mutationFn: (values: FormValues) =>
      requirementsApi.save(session!.accessToken, projectId, {
        ...values,
        constraints: draft.constraints
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["workspace-requirements", projectId] });
    }
  });

  function persistDraft() {
    update(getValues());
  }

  async function submit(values: FormValues) {
    update(values);
    await saveMutation.mutateAsync(values);
  }

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Requirement wizard"
        title="Capture baseline requirements"
        description="Use the wizard to structure the initiative summary and the requirement statements that drive downstream analysis."
        actions={
          <>
            <button className="ghost-button" onClick={persistDraft}>
              Save local draft
            </button>
            <button onClick={handleSubmit(submit)} disabled={saveMutation.isPending}>
              {saveMutation.isPending ? "Saving..." : "Save to workspace"}
            </button>
          </>
        }
      />

      <nav className="tab-nav" aria-label="Requirement wizard steps">
        {["Context", "Requirements", "Review"].map((label, index) => (
          <button key={label} className={step === index ? "tab-link tab-link--active" : "tab-link"} onClick={() => setStep(index)} type="button">
            {label}
          </button>
        ))}
      </nav>

      <form className="stack" onSubmit={handleSubmit(submit)}>
        {step === 0 && (
          <Panel title="Initiative context" subtitle="Anchor the design analysis with a strong summary.">
            <div className="stack">
              <label>
                Requirement set name
                <input {...register("name")} />
                {errors.name && <span className="form-error">{errors.name.message}</span>}
              </label>
              <label>
                Summary
                <textarea {...register("summary")} />
                {errors.summary && <span className="form-error">{errors.summary.message}</span>}
              </label>
            </div>
          </Panel>
        )}

        {step === 1 && (
          <Panel
            title="Requirement statements"
            subtitle="Capture functional and non-functional statements that will feed recommendation and UML generation."
            actions={
              <button
                className="ghost-button"
                type="button"
                onClick={() =>
                  requirementFields.append({
                    code: `REQ-${Math.random().toString(36).slice(2, 6).toUpperCase()}`,
                    title: "",
                    description: "",
                    type: 1,
                    priority: 2
                  })
                }
              >
                Add requirement
              </button>
            }
          >
            <div className="stack">
              {requirementFields.fields.map((field, index) => (
                <div key={field.id} className="item-card">
                  <div className="form-grid">
                    <label>
                      Code
                      <input {...register(`requirements.${index}.code`)} />
                    </label>
                    <label>
                      Title
                      <input {...register(`requirements.${index}.title`)} />
                    </label>
                  </div>
                  <label>
                    Description
                    <textarea {...register(`requirements.${index}.description`)} />
                  </label>
                  <div className="form-grid">
                    <label>
                      Type
                      <select {...register(`requirements.${index}.type`, { valueAsNumber: true })}>
                        {Object.values(RequirementType)
                          .filter((value) => typeof value === "number")
                          .map((value) => (
                            <option key={value} value={value}>
                              {requirementTypeLabels[value as number]}
                            </option>
                          ))}
                      </select>
                    </label>
                    <label>
                      Priority
                      <select {...register(`requirements.${index}.priority`, { valueAsNumber: true })}>
                        {Object.values(RequirementPriority)
                          .filter((value) => typeof value === "number")
                          .map((value) => (
                            <option key={value} value={value}>
                              {requirementPriorityLabels[value as number]}
                            </option>
                          ))}
                      </select>
                    </label>
                  </div>
                </div>
              ))}
            </div>
          </Panel>
        )}

        {step === 2 && (
          <Panel title="Review baseline" subtitle="Confirm what will be persisted and used by the recommendation engine.">
            <div className="stack">
              <div className="stat-tile">
                <span className="subtle-text">Summary</span>
                <strong>{getValues("summary") || "No summary yet"}</strong>
              </div>
              <div className="stat-tile">
                <span className="subtle-text">Requirements</span>
                <strong>{getValues("requirements").length}</strong>
              </div>
              <div className="stat-tile">
                <span className="subtle-text">Constraints in shared draft</span>
                <strong>{draft.constraints.length}</strong>
              </div>
            </div>
          </Panel>
        )}
      </form>
    </div>
  );
}
