import { useQueries } from "@tanstack/react-query";
import { useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import { useAuth } from "../../auth/context/AuthProvider";
import { useWorkspaceSnapshot } from "../../projects/hooks/useWorkspaceSnapshot";
import { artifactsApi } from "../../../shared/api/services/artifacts.api";
import { artifactKindLabels, OutputFormat, outputFormatLabels } from "../../../shared/types/domain";
import { downloadAuthenticatedFile } from "../../../shared/utils/downloads";
import { EmptyState } from "../../../shared/ui/EmptyState";
import { PageHeader } from "../../../shared/ui/PageHeader";
import { Panel } from "../../../shared/ui/Panel";

export function ExportsPage() {
  const { projectId = "" } = useParams();
  const { session } = useAuth();
  const snapshot = useWorkspaceSnapshot(projectId);
  const [lastExport, setLastExport] = useState<{ fileName: string; format: number; createdAtUtc: string } | null>(null);
  const [selectedFormats, setSelectedFormats] = useState<Record<string, number>>({});

  const versionQueries = useQueries({
    queries: (snapshot.artifacts.data ?? []).map((artifact) => ({
      queryKey: ["artifact-export-versions", projectId, artifact.id],
      queryFn: () => artifactsApi.versions(session!.accessToken, projectId, artifact.id),
      enabled: Boolean(session?.accessToken && projectId)
    }))
  });

  const exportableArtifacts = useMemo(
    () =>
      (snapshot.artifacts.data ?? []).map((artifact, index) => ({
        artifact,
        latestVersion: versionQueries[index]?.data?.[0]
      })),
    [snapshot.artifacts.data, versionQueries]
  );

  async function exportArtifact(artifactId: string) {
    const selected = exportableArtifacts.find((item) => item.artifact.id === artifactId);
    if (!selected?.latestVersion) {
      return;
    }

    const format = selectedFormats[artifactId] ?? getPreferredExportFormat(selected.artifact.artifactKind, selected.latestVersion.primaryFormat);
    const result = await artifactsApi.export(session!.accessToken, selected.latestVersion.id, format);
    await downloadAuthenticatedFile(result.downloadUrl, session!.accessToken, result.fileName);
    setLastExport({
      fileName: result.fileName,
      format: result.format,
      createdAtUtc: result.createdAtUtc
    });
  }

  return (
    <div className="stack">
      <PageHeader
        eyebrow="Exports"
        title="Download design outputs"
        description="Export the latest artifact versions as shareable PDF files, with source formats still available where useful."
      />

      {lastExport && (
        <section className="stats-grid">
          <div className="stat-tile">
            <span className="subtle-text">Last export</span>
            <strong>{lastExport.fileName}</strong>
          </div>
          <div className="stat-tile">
            <span className="subtle-text">Format</span>
            <strong>{outputFormatLabels[lastExport.format] ?? "Text"}</strong>
          </div>
          <div className="stat-tile">
            <span className="subtle-text">Created</span>
            <strong>{new Date(lastExport.createdAtUtc).toLocaleString()}</strong>
          </div>
        </section>
      )}

      <Panel title="Latest artifact exports" subtitle="Each row uses the newest persisted artifact version available in the workspace.">
        {exportableArtifacts.length ? (
          <div className="stack">
            {exportableArtifacts.map(({ artifact, latestVersion }) => (
              <div key={artifact.id} className="item-card">
                <div className="panel-header">
                  <div>
                    <strong>{artifact.title}</strong>
                    <p className="subtle-text">{artifactKindLabels[artifact.artifactKind] ?? "Artifact"}</p>
                  </div>
                  <span className="subtle-text">
                    {latestVersion ? `Version ${latestVersion.versionNumber}` : "No version available"}
                  </span>
                </div>
                {latestVersion ? (
                  <div className="form-grid">
                    <label>
                      Export format
                      <select
                        value={selectedFormats[artifact.id] ?? getPreferredExportFormat(artifact.artifactKind, latestVersion.primaryFormat)}
                        onChange={(event) =>
                          setSelectedFormats((current) => ({
                            ...current,
                            [artifact.id]: Number(event.target.value)
                          }))
                        }
                      >
                        {getExportOptions(artifact.artifactKind, latestVersion.primaryFormat).map((format) => (
                          <option key={format} value={format}>
                            {outputFormatLabels[format] ?? `Format ${format}`}
                          </option>
                        ))}
                      </select>
                    </label>
                    <div className="stack">
                      <span className="subtle-text">Summary</span>
                      <strong>{latestVersion.summary}</strong>
                    </div>
                  </div>
                ) : (
                  <p className="subtle-text">Generate this artifact before exporting it.</p>
                )}
                <div className="button-row">
                  <button type="button" onClick={() => void exportArtifact(artifact.id)} disabled={!latestVersion}>
                    Download latest version
                  </button>
                </div>
              </div>
            ))}
          </div>
        ) : (
          <EmptyState
            title="No artifacts available for export"
            description="Generate at least one artifact first, then use this page to download the latest version."
          />
        )}
      </Panel>
    </div>
  );
}

function getExportOptions(artifactKind: number, primaryFormat: number) {
  const formats = [OutputFormat.Pdf];

  if (
    artifactKind === 1 ||
    artifactKind === 2 ||
    artifactKind === 3 ||
    artifactKind === 4 ||
    artifactKind === 5 ||
    artifactKind === 6
  ) {
    formats.push(OutputFormat.PlantUml, OutputFormat.Mermaid);
    return Array.from(new Set(formats));
  }

  if (artifactKind === 7 || artifactKind === 8 || artifactKind === 9) {
    formats.push(OutputFormat.Mermaid);
    return Array.from(new Set(formats));
  }

  formats.push(primaryFormat);
  return Array.from(new Set(formats));
}

function getPreferredExportFormat(_artifactKind: number, _primaryFormat: number) {
  return OutputFormat.Pdf;
}
