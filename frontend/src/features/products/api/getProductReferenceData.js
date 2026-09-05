/**
 * @typedef {import("./productReferenceDataTypes.js").ProductReferenceData}
 * ProductReferenceData
 */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @returns {Promise<ProductReferenceData>}
 */
export function getProductReferenceData() {
  return apiRequest("/api/v1/catalog/products/reference-data", {
    method: "GET",
  });
}
