/** @typedef {import("./taxCategoryListTypes.js").ListTaxCategoriesParameters} ListTaxCategoriesParameters */
/** @typedef {import("./taxCategoryListTypes.js").ListTaxCategoriesResponse} ListTaxCategoriesResponse */

import { apiRequest } from "../../../shared/api/apiClient";

/**
 * @param {ListTaxCategoriesParameters} parameters
 * @returns {Promise<ListTaxCategoriesResponse>}
 */
export function listTaxCategories(parameters) {
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
    `/api/v1/catalog/tax-categories?${searchParameters.toString()}`,
    { method: "GET" },
  );
}
