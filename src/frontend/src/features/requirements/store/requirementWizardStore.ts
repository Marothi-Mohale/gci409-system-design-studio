import { create } from "zustand";
import type { ConstraintInput, RequirementInput, RequirementSetVersion } from "../../../shared/types/api";

type RequirementWizardDraft = {
  projectId: string | null;
  name: string;
  summary: string;
  requirements: RequirementInput[];
  constraints: ConstraintInput[];
};

type RequirementWizardState = {
  draft: RequirementWizardDraft;
  hydrate: (projectId: string, source?: RequirementSetVersion | null) => void;
  update: (updater: Partial<RequirementWizardDraft>) => void;
};

const defaultRequirement = (): RequirementInput => ({
  code: `REQ-${Math.random().toString(36).slice(2, 6).toUpperCase()}`,
  title: "",
  description: "",
  type: 1,
  priority: 2
});

const defaultConstraint = (): ConstraintInput => ({
  title: "",
  description: "",
  type: 2,
  severity: 2
});

export const useRequirementWizardStore = create<RequirementWizardState>((set, get) => ({
  draft: {
    projectId: null,
    name: "Baseline Requirements",
    summary: "",
    requirements: [defaultRequirement()],
    constraints: [defaultConstraint()]
  },
  hydrate(projectId, source) {
    const current = get().draft;
    if (current.projectId === projectId && current.summary && !source) {
      return;
    }

    set({
      draft: {
        projectId,
        name: source?.name ?? "Baseline Requirements",
        summary: source?.summary ?? "",
        requirements: source?.requirements.length ? source.requirements : [defaultRequirement()],
        constraints: source?.constraints.length ? source.constraints : [defaultConstraint()]
      }
    });
  },
  update(updater) {
    set((state) => ({
      draft: {
        ...state.draft,
        ...updater
      }
    }));
  }
}));
