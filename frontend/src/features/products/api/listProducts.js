/** @typedef {import('./productListTypes.js').ListProductsParameters} ListProductsParameters */

/** @typedef {import('./productListTypes.js').ListProductsResponse} ListProductsResponse */

import { apiRequest } from "../../../shared/api/apiClient";
/**
 * @param {ListProductsParameters} parameters
 * @returns {Promise<ListProductsResponse>}
 */
export async function listProducts(parameters) {
  const searchParameter = new URLSearchParams();
  const nomalizedSearch = parameters.search?.trim();
  if (nomalizedSearch) {
    searchParameter.set("search", nomalizedSearch);
  }
  if (parameters.status) {
    searchParameter.set("status", parameters.status);
  }
  searchParameter.set("pageNumber", parameters.pageNumber.toString());
  searchParameter.set("pageSize", parameters.pageSize.toString());
  const queryString = searchParameter.toString();
  return apiRequest(`/api/v1/catalog/products?${queryString}`, {
    method: "GET",
  });
}
