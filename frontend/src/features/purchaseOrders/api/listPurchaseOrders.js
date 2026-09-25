/** @typedef {import("./purchaseOrderTypes.js").ListPurchaseOrdersResponse} ListPurchaseOrdersResponse */
import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {{search?: string, status?: string, pageNumber: number, pageSize: number}} parameters
 * @returns {Promise<ListPurchaseOrdersResponse>}
 */
export function listPurchaseOrders(parameters) {
  const query = new URLSearchParams({
    pageNumber: String(parameters.pageNumber),
    pageSize: String(parameters.pageSize),
  });
  if (parameters.search) query.set("search", parameters.search);
  if (parameters.status) query.set("status", parameters.status);
  return apiRequest(`/api/v1/purchasing/purchase-orders?${query}`, {
    method: "GET",
  });
}
