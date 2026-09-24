/** @typedef {import("./productCategoryStatusActionTypes.js").ProductCategoryStatusActionResponse} ProductCategoryStatusActionResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {string} productCategoryId
 * @param {string} rowVersion
 * @returns {Promise<ProductCategoryStatusActionResponse>}
 */
export async function deactivateProductCategory(productCategoryId, rowVersion) {
  return apiRequest(
    `/api/v1/catalog/product-categories/${encodeURIComponent(productCategoryId)}/deactivate`,
    {
      method: "POST",
      body: JSON.stringify({ rowVersion }),
    },
  );
}
