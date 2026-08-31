/** @typedef {import('./productDetailsTypes.js').ProductDetails} ProductDetails */

import { apiRequest } from "../../../shared/api/apiClient";
/**
 * @param {string} productId
 * @returns {Promise<ProductDetails>}
 */
export async function getProductById(productId) {
  return apiRequest(
    `/api/v1/catalog/products/${encodeURIComponent(productId)}`,
    {
      method: "GET",
    },
  );
}
