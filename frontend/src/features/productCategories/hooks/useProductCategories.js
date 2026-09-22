/**
 * @typedef {import("../api/productCategoryListTypes.js").ListProductCategoriesParameters}
 * ListProductCategoriesParameters
 */

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { listProductCategories } from "../api/listProductCategories";

/**
 * @param {ListProductCategoriesParameters} parameters
 */
export function useProductCategories(parameters) {
  return useQuery({
    queryKey: ["product-categories", "list", parameters],
    queryFn: () => listProductCategories(parameters),
    staleTime: 30_000,
    placeholderData: keepPreviousData,
  });
}
