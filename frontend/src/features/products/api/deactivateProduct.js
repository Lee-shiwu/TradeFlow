/** @typedef {import ("./productStatusActionTypes").ProductStatusActionResponse} ProductStatusActionResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * 禁用指定商品
 *
 * @param {string} productId
 * @param {string} rowVersion
 * @returns {Promise<ProductStatusActionResponse>}
 *
 */

export async function deactivateProduct(productId, rowVersion) {
  return apiRequest(
    `/api/v1/catalog/products/${encodeURIComponent(productId)}/deactivate`,
    {
      method: "POST",
      body: JSON.stringify({
        rowVersion,
      }),
    },
  );
}
