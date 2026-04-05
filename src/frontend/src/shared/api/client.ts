import { env } from "../config/env";

export class ApiError extends Error {
  constructor(
    message: string,
    public readonly status: number
  ) {
    super(message);
  }
}

type RequestOptions = {
  method?: "GET" | "POST";
  token?: string | null;
  body?: unknown;
};

export async function apiRequest<T>(path: string, options: RequestOptions = {}): Promise<T> {
  let response: Response;

  try {
    const headers: Record<string, string> = {
      ...(options.token ? { Authorization: `Bearer ${options.token}` } : {})
    };

    if (options.body) {
      headers["Content-Type"] = "application/json";
    }

    response = await fetch(`${env.apiBaseUrl}${path}`, {
      method: options.method ?? "GET",
      headers,
      body: options.body ? JSON.stringify(options.body) : undefined
    });
  } catch (error) {
    throw new ApiError(
      error instanceof Error ? `Unable to reach the gci409 API. ${error.message}` : "Unable to reach the gci409 API.",
      0
    );
  }

  const responseText = response.status === 204 ? "" : await response.text();

  if (!response.ok) {
    let message = `Request failed with status ${response.status}.`;

    try {
      const problem = responseText
        ? (JSON.parse(responseText) as { detail?: string; title?: string; errors?: Record<string, string[]> })
        : undefined;
      const validationMessage = problem?.errors
        ? Object.values(problem.errors)
            .flat()
            .find(Boolean)
        : undefined;

      message = validationMessage ?? problem?.detail ?? problem?.title ?? message;
    } catch {
      if (responseText) {
        message = responseText;
      }
    }

    throw new ApiError(message, response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  if (!responseText) {
    return undefined as T;
  }

  return JSON.parse(responseText) as T;
}
