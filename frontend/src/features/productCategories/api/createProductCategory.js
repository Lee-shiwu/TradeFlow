/** @typedef {import("./createProductCategoryTypes.js").CreateProductCategoryRequest} CreateProductCategoryRequest */

/** @typedef {import("./createProductCategoryTypes.js").CreateProductCategoryResponse} CreateProductCategoryResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {CreateProductCategoryRequest} request
 * @returns {Promise<CreateProductCategoryResponse>}
 */
export function createProductCategory(request) {
  return apiRequest("/api/v1/catalog/product-categories", {
    method: "POST",
    body: JSON.stringify(request),
  });
}
