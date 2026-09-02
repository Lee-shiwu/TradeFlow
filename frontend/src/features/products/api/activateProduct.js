/** @typedef {import("./productStatusActionTypes.js").ProductStatusActionResponse} ProductStatusActionResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * 启用指定商品。
 *
 * @param {string} productId
 * @param {string} rowVersion
 * @returns {Promise<ProductStatusActionResponse>}
 */
export async function activateProduct(productId, rowVersion) {
  return apiRequest(
    `/api/v1/catalog/products/${encodeURIComponent(productId)}/activate`,
    {
      method: "POST",
      body: JSON.stringify({
        rowVersion,
      }),
    },
  );
}
