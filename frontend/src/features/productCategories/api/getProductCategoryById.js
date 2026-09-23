/** @typedef {import("./productCategoryDetailsTypes.js").ProductCategoryDetails} ProductCategoryDetails */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {string} productCategoryId
 * @returns {Promise<ProductCategoryDetails>}
 */
export function getProductCategoryById(productCategoryId) {
  return apiRequest(
    `/api/v1/catalog/product-categories/${encodeURIComponent(productCategoryId)}`,
    { method: "GET" },
  );
}
