export enum ProjectRole {
  Owner = 1,
  Contributor = 2,
  Reviewer = 3,
  Viewer = 4
}

export enum ArtifactKind {
  UseCaseDiagram = 1,
  ClassDiagram = 2,
  SequenceDiagram = 3,
  ActivityDiagram = 4,
  ComponentDiagram = 5,
  DeploymentDiagram = 6,
  ContextDiagram = 7,
  DataFlowDiagram = 8,
  Erd = 9,
  ArchitectureSummary = 10,
  ModuleDecomposition = 11,
  ApiDesignSuggestion = 12,
  DatabaseDesignSuggestion = 13
}

export enum OutputFormat {
  Markdown = 1,
  Mermaid = 2,
  PlantUml = 3,
  Pdf = 4,
  Png = 5
}

export enum RequirementType {
  Functional = 1,
  NonFunctional = 2,
  Integration = 3,
  Security = 4,
  Data = 5,
  Reporting = 6
}

export enum RequirementPriority {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4
}

export enum ConstraintType {
  Business = 1,
  Technical = 2,
  Regulatory = 3,
  Cost = 4,
  Timeline = 5,
  Platform = 6
}

export enum ConstraintSeverity {
  Advisory = 1,
  Important = 2,
  Mandatory = 3
}

export enum GenerationStatus {
  Queued = 1,
  Processing = 2,
  Completed = 3,
  Failed = 4
}

export enum ArtifactStatus {
  Draft = 1,
  Reviewed = 2,
  Approved = 3,
  Superseded = 4
}

export const outputFormatLabels: Record<number, string> = {
  [OutputFormat.Markdown]: "Markdown",
  [OutputFormat.Mermaid]: "Mermaid",
  [OutputFormat.PlantUml]: "PlantUML",
  [OutputFormat.Pdf]: "PDF",
  [OutputFormat.Png]: "PNG"
};

export const projectRoleLabels: Record<number, string> = {
  [ProjectRole.Owner]: "Owner",
  [ProjectRole.Contributor]: "Contributor",
  [ProjectRole.Reviewer]: "Reviewer",
  [ProjectRole.Viewer]: "Viewer"
};

export const artifactKindLabels: Record<number, string> = {
  [ArtifactKind.UseCaseDiagram]: "Use Case Diagram",
  [ArtifactKind.ClassDiagram]: "Class Diagram",
  [ArtifactKind.SequenceDiagram]: "Sequence Diagram",
  [ArtifactKind.ActivityDiagram]: "Activity Diagram",
  [ArtifactKind.ComponentDiagram]: "Component Diagram",
  [ArtifactKind.DeploymentDiagram]: "Deployment Diagram",
  [ArtifactKind.ContextDiagram]: "Context Diagram",
  [ArtifactKind.DataFlowDiagram]: "DFD",
  [ArtifactKind.Erd]: "ERD",
  [ArtifactKind.ArchitectureSummary]: "Architecture Summary",
  [ArtifactKind.ModuleDecomposition]: "Module Decomposition",
  [ArtifactKind.ApiDesignSuggestion]: "API Design Suggestion",
  [ArtifactKind.DatabaseDesignSuggestion]: "Database Design Suggestion"
};

export const requirementTypeLabels: Record<number, string> = {
  [RequirementType.Functional]: "Functional",
  [RequirementType.NonFunctional]: "Non-functional",
  [RequirementType.Integration]: "Integration",
  [RequirementType.Security]: "Security",
  [RequirementType.Data]: "Data",
  [RequirementType.Reporting]: "Reporting"
};

export const requirementPriorityLabels: Record<number, string> = {
  [RequirementPriority.Low]: "Low",
  [RequirementPriority.Medium]: "Medium",
  [RequirementPriority.High]: "High",
  [RequirementPriority.Critical]: "Critical"
};

export const constraintTypeLabels: Record<number, string> = {
  [ConstraintType.Business]: "Business",
  [ConstraintType.Technical]: "Technical",
  [ConstraintType.Regulatory]: "Regulatory",
  [ConstraintType.Cost]: "Cost",
  [ConstraintType.Timeline]: "Timeline",
  [ConstraintType.Platform]: "Platform"
};

export const constraintSeverityLabels: Record<number, string> = {
  [ConstraintSeverity.Advisory]: "Advisory",
  [ConstraintSeverity.Important]: "Important",
  [ConstraintSeverity.Mandatory]: "Mandatory"
};

export const generationStatusLabels: Record<number, string> = {
  [GenerationStatus.Queued]: "Queued",
  [GenerationStatus.Processing]: "Processing",
  [GenerationStatus.Completed]: "Completed",
  [GenerationStatus.Failed]: "Failed"
};

export const artifactStatusLabels: Record<number, string> = {
  [ArtifactStatus.Draft]: "Draft",
  [ArtifactStatus.Reviewed]: "Reviewed",
  [ArtifactStatus.Approved]: "Approved",
  [ArtifactStatus.Superseded]: "Superseded"
};

export function roleAllows(currentRole: number | null | undefined, requiredRole: ProjectRole) {
  return typeof currentRole === "number" && currentRole <= requiredRole;
}
