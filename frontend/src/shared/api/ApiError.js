export class ApiError extends Error {
  status;
  code;
  detail;
  traceId;
  errors;
  constructor(status, code, detail, traceId, errors) {
    super(detail);
    this.name = "ApiError";
    this.status = status;
    this.code = code;
    this.detail = detail;
    this.traceId = traceId;
    this.errors = errors;
  }
}
