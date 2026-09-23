/** @typedef {import("./updateProductCategoryDetailsTypes.js").UpdateProductCategoryDetailsRequest} UpdateProductCategoryDetailsRequest */
/** @typedef {import("./updateProductCategoryDetailsTypes.js").UpdateProductCategoryDetailsResponse} UpdateProductCategoryDetailsResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {string} productCategoryId
 * @param {UpdateProductCategoryDetailsRequest} request
 * @returns {Promise<UpdateProductCategoryDetailsResponse>}
 */
export function updateProductCategoryDetails(productCategoryId, request) {
  return apiRequest(
    `/api/v1/catalog/product-categories/${encodeURIComponent(productCategoryId)}`,
    {
      method: "PUT",
      body: JSON.stringify(request),
    },
  );
}
