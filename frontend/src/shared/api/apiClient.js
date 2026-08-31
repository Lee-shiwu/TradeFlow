/** @typedef {import('./ApiProblem.js').ApiProblem} ApiProblem */

import { getRuntimeConfig } from "../config/runtimeConfig";
import { ApiError } from "./ApiError";
/**
 * @template TResponse
 * @param {string} path
 * @param {RequestInit} [options]
 * @returns {Promise<TResponse>}
 */
export async function apiRequest(path, options) {
  const config = getRuntimeConfig();
  const headers = new Headers(options?.headers);
  headers.set("Accept", "application/json");
  headers.set("X-Organisation-Id", config.temporaryOrganisationId);
  headers.set("X-User-Id", config.temporaryUserId);
  if (options?.body != null && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }
  const response = await fetch(path, {
    ...options,
    headers,
  });
  if (response.ok) {
    if (response.status === 204) {
      return undefined;
    }
    return await response.json();
  }
  let problem;
  try {
    const responseBody = await response.json();
    problem = responseBody;
  } catch {
    throw new ApiError(
      response.status,
      "HTTP_ERROR",
      `Request failed with status ${response.status}.`,
    );
  }
  const status =
    typeof problem.status === "number" ? problem.status : response.status;
  const code =
    typeof problem.code === "string" && problem.code.trim().length > 0
      ? problem.code
      : "HTTP_ERROR";
  const detail =
    typeof problem.detail === "string" && problem.detail.trim().length > 0
      ? problem.detail
      : typeof problem.title === "string" && problem.title.trim().length > 0
        ? problem.title
        : `Request failed with status ${response.status}.`;
  const traceId =
    typeof problem.traceId === "string" ? problem.traceId : undefined;
  throw new ApiError(status, code, detail, traceId, problem.errors);
}
