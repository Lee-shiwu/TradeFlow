/**
 * @typedef {import("./productCategoryListTypes.js").ListProductCategoriesParameters}
 * ListProductCategoriesParameters
 */

/**
 * @typedef {import("./productCategoryListTypes.js").ListProductCategoriesResponse}
 * ListProductCategoriesResponse
 */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {ListProductCategoriesParameters} parameters
 * @returns {Promise<ListProductCategoriesResponse>}
 */
export function listProductCategories(parameters) {
  const searchParameters = new URLSearchParams();
  const normalizedSearch = parameters.search?.trim();

  if (normalizedSearch) {
    searchParameters.set("search", normalizedSearch);
  }

  if (parameters.status) {
    searchParameters.set("status", parameters.status);
  }

  searchParameters.set("pageNumber", parameters.pageNumber.toString());
  searchParameters.set("pageSize", parameters.pageSize.toString());

  return apiRequest(
    `/api/v1/catalog/product-categories?${searchParameters.toString()}`,
    { method: "GET" },
  );
}
