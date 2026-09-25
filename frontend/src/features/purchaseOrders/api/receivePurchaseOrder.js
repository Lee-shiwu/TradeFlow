import { apiRequest } from "../../../shared/api/apiClient";

export function receivePurchaseOrder(purchaseOrderId, rowVersion) {
  return apiRequest(
    `/api/v1/purchasing/purchase-orders/${encodeURIComponent(purchaseOrderId)}/receive`,
    {
      method: "POST",
      body: JSON.stringify({ rowVersion }),
    },
  );
}
