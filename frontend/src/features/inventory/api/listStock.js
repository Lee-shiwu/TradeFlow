import { apiRequest } from "../../../shared/api/apiClient";

export function listStock(parameters) {
  const query = new URLSearchParams({
    pageNumber: String(parameters.pageNumber),
    pageSize: String(parameters.pageSize),
  });
  if (parameters.search) query.set("search", parameters.search);
  return apiRequest(`/api/v1/inventory/stock?${query}`, { method: "GET" });
}
