/** @typedef {import("./productCategoryStatusActionTypes.js").ProductCategoryStatusActionResponse} ProductCategoryStatusActionResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {string} productCategoryId
 * @param {string} rowVersion
 * @returns {Promise<ProductCategoryStatusActionResponse>}
 */
export async function activateProductCategory(productCategoryId, rowVersion) {
  return apiRequest(
    `/api/v1/catalog/product-categories/${encodeURIComponent(productCategoryId)}/activate`,
    {
      method: "POST",
      body: JSON.stringify({ rowVersion }),
    },
  );
}
