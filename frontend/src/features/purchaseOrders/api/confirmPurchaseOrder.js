import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {string} purchaseOrderId
 * @param {string} rowVersion
 */
export function confirmPurchaseOrder(purchaseOrderId, rowVersion) {
  return apiRequest(
    `/api/v1/purchasing/purchase-orders/${encodeURIComponent(purchaseOrderId)}/confirm`,
    {
      method: "POST",
      body: JSON.stringify({ rowVersion }),
    },
  );
}
