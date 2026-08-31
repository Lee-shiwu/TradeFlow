/** @typedef {import('../api/productListTypes.js').ListProductsParameters} ListProductsParameters */

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { listProducts } from "../api/listProducts";
/**
 * @param {ListProductsParameters} parameters
 */
export function useProducts(parameters) {
  return useQuery({
    queryKey: ["products", "list", parameters],
    queryFn: () => listProducts(parameters),
    staleTime: 30_000,
    placeholderData: keepPreviousData,
  });
}
