export type AuthResponse = {
  userId: string;
  fullName: string;
  email: string;
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
};

export type ProjectSummary = {
  id: string;
  key: string;
  name: string;
  description: string;
  status: number;
  role: number;
  createdAtUtc: string;
};

export type ProjectDetail = {
  id: string;
  key: string;
  name: string;
  description: string;
  status: number;
  members: { userId: string; role: number; status: number }[];
};

export type RequirementInput = {
  code: string;
  title: string;
  description: string;
  type: number;
  priority: number;
};

export type ConstraintInput = {
  title: string;
  description: string;
  type: number;
  severity: number;
};

export type RequirementSetVersion = {
  requirementSetId: string;
  versionId: string;
  name: string;
  versionNumber: number;
  summary: string;
  requirements: RequirementInput[];
  constraints: ConstraintInput[];
  createdAtUtc: string;
};

export type RecommendationItem = {
  artifactKind: number;
  title: string;
  rationale: string;
  confidenceScore: number;
  strength: number;
};

export type RecommendationSet = {
  recommendationSetId: string;
  requirementSetVersionId: string;
  items: RecommendationItem[];
  createdAtUtc: string;
};

export type GenerationRequest = {
  id: string;
  requirementSetVersionId: string;
  status: number;
  targets: { artifactKind: number; preferredFormat: number }[];
  createdAtUtc: string;
  completedAtUtc?: string | null;
  failureReason?: string | null;
};

export type ArtifactSummary = {
  id: string;
  artifactKind: number;
  title: string;
  status: number;
  currentVersionNumber: number;
  createdAtUtc: string;
};

export type ArtifactVersion = {
  id: string;
  versionNumber: number;
  primaryFormat: number;
  summary: string;
  content: string;
  createdAtUtc: string;
};

export type ExportResponse = {
  id: string;
  format: number;
  fileName: string;
  content?: string | null;
  contentType: string;
  contentEncoding?: string | null;
  downloadUrl: string;
  createdAtUtc: string;
};
