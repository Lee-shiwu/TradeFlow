/** @typedef {import("./purchaseOrderTypes.js").CreatePurchaseOrderRequest} CreatePurchaseOrderRequest */
import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {CreatePurchaseOrderRequest} request
 */
export function createPurchaseOrder(request) {
  return apiRequest("/api/v1/purchasing/purchase-orders", {
    method: "POST",
    body: JSON.stringify(request),
  });
}
