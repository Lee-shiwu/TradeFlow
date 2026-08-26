export class ApiError extends Error {
  public readonly status: number
  public readonly code: string
  public readonly detail: string
  public readonly traceId?: string
  public readonly errors?: Record<string, string[]>

  public constructor(
    status: number,
    code: string,
    detail: string,
    traceId?: string,
    errors?: Record<string, string[]>,
  ) {
    super(detail)

    this.name = 'ApiError'
    this.status = status
    this.code = code
    this.detail = detail
    this.traceId = traceId
    this.errors = errors
  }
}
