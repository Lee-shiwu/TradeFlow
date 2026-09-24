/** @typedef {import("../api/taxCategoryListTypes.js").ListTaxCategoriesParameters} ListTaxCategoriesParameters */

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { listTaxCategories } from "../api/listTaxCategories";

/**
 * @param {ListTaxCategoriesParameters} parameters
 */
export function useTaxCategories(parameters) {
  return useQuery({
    queryKey: ["tax-categories", "list", parameters],
    queryFn: () => listTaxCategories(parameters),
    staleTime: 30_000,
    placeholderData: keepPreviousData,
  });
}
