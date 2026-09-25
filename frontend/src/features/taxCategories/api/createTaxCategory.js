/** @typedef {import("./createTaxCategoryTypes.js").CreateTaxCategoryRequest} CreateTaxCategoryRequest */
/** @typedef {import("./createTaxCategoryTypes.js").CreateTaxCategoryResponse} CreateTaxCategoryResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {CreateTaxCategoryRequest} request
 * @returns {Promise<CreateTaxCategoryResponse>}
 */
export function createTaxCategory(request) {
  return apiRequest("/api/v1/catalog/tax-categories", {
    method: "POST",
    body: JSON.stringify(request),
  });
}
