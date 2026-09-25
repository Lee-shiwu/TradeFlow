/** @typedef {import("./supplierTypes.js").ListSuppliersParameters} ListSuppliersParameters */
/** @typedef {import("./supplierTypes.js").ListSuppliersResponse} ListSuppliersResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {ListSuppliersParameters} parameters
 * @returns {Promise<ListSuppliersResponse>}
 */
export function listSuppliers(parameters) {
  const searchParameters = new URLSearchParams({
    pageNumber: String(parameters.pageNumber),
    pageSize: String(parameters.pageSize),
  });

  if (parameters.search) {
    searchParameters.set("search", parameters.search);
  }

  if (parameters.status) {
    searchParameters.set("status", parameters.status);
  }

  return apiRequest(
    `/api/v1/purchasing/suppliers?${searchParameters.toString()}`,
    { method: "GET" },
  );
}
