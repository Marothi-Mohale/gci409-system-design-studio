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
    response = await fetch(`${env.apiBaseUrl}${path}`, {
      method: options.method ?? "GET",
      headers: {
        "Content-Type": "application/json",
        ...(options.token ? { Authorization: `Bearer ${options.token}` } : {})
      },
      body: options.body ? JSON.stringify(options.body) : undefined
    });
  } catch (error) {
    throw new ApiError(
      error instanceof Error ? `Unable to reach the gci409 API. ${error.message}` : "Unable to reach the gci409 API.",
      0
    );
  }

  if (!response.ok) {
    let message = `Request failed with status ${response.status}.`;

    try {
      const problem = (await response.json()) as { detail?: string; title?: string; errors?: Record<string, string[]> };
      const validationMessage = problem.errors
        ? Object.values(problem.errors)
            .flat()
            .find(Boolean)
        : undefined;

      message = validationMessage ?? problem.detail ?? problem.title ?? message;
    } catch {
      const fallback = await response.text();
      if (fallback) {
        message = fallback;
      }
    }

    throw new ApiError(message, response.status);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
