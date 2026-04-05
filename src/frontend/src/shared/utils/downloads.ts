import { env } from "../config/env";

export function downloadTextFile(fileName: string, content: string, mimeType = "text/plain;charset=utf-8") {
  const blob = new Blob([content], { type: mimeType });
  triggerBrowserDownload(blob, fileName);
}

export async function downloadAuthenticatedFile(downloadPath: string, token: string, fallbackFileName?: string) {
  const response = await fetch(resolveDownloadUrl(downloadPath), {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  if (!response.ok) {
    throw new Error(`Unable to download the export. The server returned ${response.status}.`);
  }

  const blob = await response.blob();
  const fileName = readFileName(response.headers.get("Content-Disposition")) ?? fallbackFileName ?? "download";
  triggerBrowserDownload(blob, fileName);
}

function triggerBrowserDownload(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}

function resolveDownloadUrl(downloadPath: string) {
  if (downloadPath.startsWith("http://") || downloadPath.startsWith("https://")) {
    return downloadPath;
  }

  return `${env.apiBaseUrl}${downloadPath}`;
}

function readFileName(contentDisposition: string | null) {
  if (!contentDisposition) {
    return null;
  }

  const utf8Match = contentDisposition.match(/filename\*=UTF-8''([^;]+)/i);
  if (utf8Match?.[1]) {
    return decodeURIComponent(utf8Match[1]);
  }

  const asciiMatch = contentDisposition.match(/filename="?([^"]+)"?/i);
  return asciiMatch?.[1] ?? null;
}
