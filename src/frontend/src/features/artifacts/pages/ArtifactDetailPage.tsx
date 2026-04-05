import { useQuery } from "@tanstack/react-query";
import { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import { useAuth } from "../../auth/context/AuthProvider";
import { useWorkspaceSnapshot } from "../../projects/hooks/useWorkspaceSnapshot";
import { artifactsApi } from "../../../shared/api/services/artifacts.api";
import {
  ArtifactKind,
  artifactKindLabels,
  artifactStatusLabels,
  ArtifactStatus,
  OutputFormat,
  outputFormatLabels
} from "../../../shared/types/domain";
import { downloadTextFile } from "../../../shared/utils/downloads";
import { DiagramPreview } from "../../../shared/ui/DiagramPreview";
import { EmptyState } from "../../../shared/ui/EmptyState";
import { LoadingBlock } from "../../../shared/ui/LoadingBlock";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";
import { StatusBadge } from "../../../shared/ui/StatusBadge";

type PreviewState = {
  format: number;
  content: string;
  fileName: string;
};

export function ArtifactDetailPage() {
  const { projectId = "", artifactId = "" } = useParams();
  const { session } = useAuth();
  const snapshot = useWorkspaceSnapshot(projectId);
  const artifact = snapshot.artifacts.data?.find((item) => item.id === artifactId);
  const [selectedVersionId, setSelectedVersionId] = useState<string | null>(null);
  const [preview, setPreview] = useState<PreviewState | null>(null);
  const [previewMessage, setPreviewMessage] = useState<string | null>(null);

  const versionsQuery = useQuery({
    queryKey: ["artifact-versions", projectId, artifactId],
    queryFn: () => artifactsApi.versions(session!.accessToken, projectId, artifactId),
    enabled: Boolean(session?.accessToken && projectId && artifactId)
  });

  const selectedVersion = useMemo(() => {
    if (!versionsQuery.data?.length) {
      return undefined;
    }

    return versionsQuery.data.find((version) => version.id === selectedVersionId) ?? versionsQuery.data[0];
  }, [selectedVersionId, versionsQuery.data]);

  useEffect(() => {
    if (!selectedVersionId && versionsQuery.data?.length) {
      setSelectedVersionId(versionsQuery.data[0].id);
    }
  }, [selectedVersionId, versionsQuery.data]);

  useEffect(() => {
    if (!artifact || !selectedVersion) {
      return;
    }

    setPreview({
      format: selectedVersion.primaryFormat,
      content: selectedVersion.content,
      fileName: buildFileName(artifact.title, selectedVersion.versionNumber, selectedVersion.primaryFormat)
    });
    setPreviewMessage(null);
  }, [artifact, selectedVersion]);

  if (versionsQuery.isLoading) {
    return <LoadingBlock label="Loading artifact detail" />;
  }

  if (!artifact) {
    return <EmptyState title="Artifact not found" description="Return to the artifact catalog and select a generated item to review." />;
  }

  if (!selectedVersion) {
    return <EmptyState title="No versions found" description="This artifact exists, but it does not have any persisted versions yet." />;
  }

  const currentVersion = selectedVersion;

  async function handleFormatSelection(format: number) {
    if (!artifact) {
      return;
    }

    if (format === currentVersion.primaryFormat) {
      setPreview({
        format,
        content: currentVersion.content,
        fileName: buildFileName(artifact.title, currentVersion.versionNumber, format)
      });
      setPreviewMessage(null);
      return;
    }

    try {
      const exportResult = await artifactsApi.export(session!.accessToken, currentVersion.id, format);
      setPreview({
        format: exportResult.format,
        content: exportResult.content,
        fileName: exportResult.fileName
      });
      setPreviewMessage(`Preview loaded from export ${outputFormatLabels[exportResult.format] ?? exportResult.format}.`);
    } catch (error) {
      setPreviewMessage(error instanceof Error ? error.message : "Unable to load the selected representation.");
    }
  }

  function downloadPreview() {
    if (!preview) {
      return;
    }

    downloadTextFile(preview.fileName, preview.content, getMimeType(preview.format));
  }

  const availableFormats = getAvailableFormats(artifact.artifactKind, currentVersion.primaryFormat);

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Artifact detail"
        title={artifact.title}
        description={artifactKindLabels[artifact.artifactKind] ?? "Artifact"}
        actions={
          <>
            <StatusBadge label={artifactStatusLabels[artifact.status] ?? "Draft"} tone={getArtifactTone(artifact.status)} />
            <button className="ghost-button" onClick={downloadPreview}>
              Download current preview
            </button>
          </>
        }
      />

      <div className="detail-grid">
        <Panel title="Versions" subtitle="Select a persisted version to inspect.">
          <div className="stack">
            {versionsQuery.data?.map((version) => {
              const isActive = version.id === selectedVersion.id;

              return (
                <button
                  key={version.id}
                  className={isActive ? "item-card tab-link--active" : "item-card"}
                  type="button"
                  onClick={() => setSelectedVersionId(version.id)}
                >
                  <strong>Version {version.versionNumber}</strong>
                  <span className="subtle-text">{outputFormatLabels[version.primaryFormat] ?? "Text"}</span>
                  <span className="subtle-text">{new Date(version.createdAtUtc).toLocaleString()}</span>
                </button>
              );
            })}
          </div>
        </Panel>

        <div className="stack">
          <Panel
            title="Preview controls"
            subtitle="Switch between the primary artifact format and available derived representations."
            actions={
              <div className="button-row">
                {availableFormats.map((format) => (
                  <button
                    key={format}
                    className={preview?.format === format ? "" : "ghost-button"}
                    type="button"
                    onClick={() => void handleFormatSelection(format)}
                  >
                    {outputFormatLabels[format] ?? `Format ${format}`}
                  </button>
                ))}
              </div>
            }
          >
            <div className="stack">
              <p className="subtle-text">{currentVersion.summary}</p>
              {previewMessage && <div className="message">{previewMessage}</div>}
            </div>
          </Panel>

          <Panel title="Artifact preview" subtitle="Rendered Mermaid where possible, otherwise raw notation source.">
            {preview ? (
              <DiagramPreview notation={toPreviewNotation(preview.format)} source={preview.content} />
            ) : (
              <EmptyState title="No preview loaded" description="Choose a representation above to inspect this artifact version." />
            )}
          </Panel>
        </div>
      </div>
    </div>
  );
}

function getAvailableFormats(artifactKind: number, primaryFormat: number) {
  if (isUmlArtifact(artifactKind)) {
    return [OutputFormat.PlantUml, OutputFormat.Mermaid];
  }

  if (
    artifactKind === ArtifactKind.ContextDiagram ||
    artifactKind === ArtifactKind.DataFlowDiagram ||
    artifactKind === ArtifactKind.Erd
  ) {
    return [OutputFormat.Mermaid];
  }

  return [primaryFormat];
}

function isUmlArtifact(artifactKind: number) {
  return (
    artifactKind === ArtifactKind.UseCaseDiagram ||
    artifactKind === ArtifactKind.ClassDiagram ||
    artifactKind === ArtifactKind.SequenceDiagram ||
    artifactKind === ArtifactKind.ActivityDiagram ||
    artifactKind === ArtifactKind.ComponentDiagram ||
    artifactKind === ArtifactKind.DeploymentDiagram
  );
}

function buildFileName(title: string, versionNumber: number, format: number) {
  const slug = title.replace(/\s+/g, "-");
  return `${slug}-v${versionNumber}.${resolveExtension(format)}`;
}

function resolveExtension(format: number) {
  switch (format) {
    case OutputFormat.Markdown:
      return "md";
    case OutputFormat.Mermaid:
      return "mmd";
    case OutputFormat.PlantUml:
      return "puml";
    case OutputFormat.Pdf:
      return "pdf";
    case OutputFormat.Png:
      return "png";
    default:
      return "txt";
  }
}

function toPreviewNotation(format: number) {
  switch (format) {
    case OutputFormat.Mermaid:
      return "mermaid" as const;
    case OutputFormat.PlantUml:
      return "plantuml" as const;
    default:
      return "markdown" as const;
  }
}

function getMimeType(format: number) {
  switch (format) {
    case OutputFormat.Markdown:
      return "text/markdown;charset=utf-8";
    case OutputFormat.Mermaid:
    case OutputFormat.PlantUml:
      return "text/plain;charset=utf-8";
    default:
      return "text/plain;charset=utf-8";
  }
}

function getArtifactTone(status: number) {
  switch (status) {
    case ArtifactStatus.Approved:
      return "success" as const;
    case ArtifactStatus.Reviewed:
      return "warning" as const;
    default:
      return "neutral" as const;
  }
}
