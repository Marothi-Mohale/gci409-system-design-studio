import mermaid from "mermaid";
import { useEffect, useId, useState } from "react";
import { CodeBlock } from "./CodeBlock";

mermaid.initialize({
  startOnLoad: false,
  securityLevel: "loose",
  theme: "neutral"
});

type DiagramPreviewProps = {
  notation: "mermaid" | "plantuml" | "markdown";
  source: string;
};

export function DiagramPreview({ notation, source }: DiagramPreviewProps) {
  const [svg, setSvg] = useState("");
  const [error, setError] = useState<string | null>(null);
  const id = useId().replace(/:/g, "");

  useEffect(() => {
    let active = true;

    if (notation !== "mermaid") {
      setSvg("");
      setError(null);
      return;
    }

    mermaid
      .render(`gci409-${id}`, source)
      .then((result) => {
        if (!active) {
          return;
        }

        setSvg(result.svg);
        setError(null);
      })
      .catch((renderError) => {
        if (!active) {
          return;
        }

        setSvg("");
        setError(renderError instanceof Error ? renderError.message : "Unable to render Mermaid preview.");
      });

    return () => {
      active = false;
    };
  }, [id, notation, source]);

  if (notation === "mermaid" && svg) {
    return <div className="diagram-surface" dangerouslySetInnerHTML={{ __html: svg }} />;
  }

  return (
    <div className="stack">
      {notation === "plantuml" && (
        <p className="subtle-text">
          PlantUML source is available here. A server-side renderer can be connected later for SVG or PNG previews.
        </p>
      )}
      {error && <div className="message">{error}</div>}
      <CodeBlock content={source} />
    </div>
  );
}
