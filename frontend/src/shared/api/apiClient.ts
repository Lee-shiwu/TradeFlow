import { getRuntimeConfig } from '../config/runtimeConfig'
import { ApiError } from './ApiError'
import type { ApiProblem } from './ApiProblem'

export async function apiRequest<TResponse>(
  path: string,
  options?: RequestInit,
): Promise<TResponse> {
  const config = getRuntimeConfig()

  const headers = new Headers(options?.headers)
  headers.set('Accept', 'application/json')
  headers.set(
    'X-Organisation-Id',
    config.temporaryOrganisationId,
  )
  headers.set('X-User-Id', config.temporaryUserId)

  if (options?.body != null && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(path, {
    ...options,
    headers,
  })

  if (response.ok) {
    if (response.status === 204) {
      return undefined as TResponse
    }

    return (await response.json()) as TResponse
  }

  let problem: Partial<ApiProblem>

  try {
    const responseBody: unknown = await response.json()
    problem = responseBody as Partial<ApiProblem>
  } catch {
    throw new ApiError(
      response.status,
      'HTTP_ERROR',
      `Request failed with status ${response.status}.`,
    )
  }

  const status =
    typeof problem.status === 'number'
      ? problem.status
      : response.status

  const code =
    typeof problem.code === 'string' &&
    problem.code.trim().length > 0
      ? problem.code
      : 'HTTP_ERROR'

  const detail =
    typeof problem.detail === 'string' &&
    problem.detail.trim().length > 0
      ? problem.detail
      : typeof problem.title === 'string' &&
          problem.title.trim().length > 0
        ? problem.title
        : `Request failed with status ${response.status}.`

  const traceId =
    typeof problem.traceId === 'string'
      ? problem.traceId
      : undefined

  throw new ApiError(
    status,
    code,
    detail,
    traceId,
    problem.errors,
  )
}
