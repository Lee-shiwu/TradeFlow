/** @typedef {import("./taxCategoryDetailsTypes.js").TaxCategoryDetails} TaxCategoryDetails */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {string} taxCategoryId
 * @returns {Promise<TaxCategoryDetails>}
 */
export function getTaxCategoryById(taxCategoryId) {
  return apiRequest(
    `/api/v1/catalog/tax-categories/${encodeURIComponent(taxCategoryId)}`,
    { method: "GET" },
  );
}
