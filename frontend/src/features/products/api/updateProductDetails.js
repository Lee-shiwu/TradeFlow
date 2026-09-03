/** @typedef {import ("./updateProductDetailsTypes").UpdateProductDetailsRequest} UpdateProductDetailsRequest */
/** @typedef {import ("./updateProductDetailsTypes").UpdateProductDetailsResponse} UpdateProductDetailsResponse*/
import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {string} productId
 * @param {UpdateProductDetailsRequest} request
 * @returns {Promise<UpdateProductDetailsResponse>}
 */

export async function updateProductDetails(productId, request) {
  return apiRequest(
    `/api/v1/catalog/products/${encodeURIComponent(productId)}`,
    {
      method: "PUT",
      body: JSON.stringify(request),
    },
  );
}
