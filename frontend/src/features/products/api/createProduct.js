/** @typedef {import  ('./createProductTypes').CreateProductRequest} CreateProductRequest */
/** @typedef {import  ('./createProductTypes').CreateProductResponse} CreateProductResponse */
import { apiRequest } from "../../../shared/api/apiClient";

/**
 *
 * @param {CreateProductRequest} request
 * @returns {Promise<CreateProductResponse>}
 */
export function createProduct(request) {
  return apiRequest(`/api/v1/catalog/products`, {
    method: `POST`,
    body: JSON.stringify(request),
  });
}
