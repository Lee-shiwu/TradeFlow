/** @typedef {import("./supplierTypes.js").CreateSupplierRequest} CreateSupplierRequest */
/** @typedef {import("./supplierTypes.js").CreateSupplierResponse} CreateSupplierResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {CreateSupplierRequest} request
 * @returns {Promise<CreateSupplierResponse>}
 */
export function createSupplier(request) {
  return apiRequest("/api/v1/purchasing/suppliers", {
    method: "POST",
    body: JSON.stringify(request),
  });
}
