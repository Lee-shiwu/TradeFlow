import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { listProducts } from "../api/listProducts";
import type { ListProductsParameters } from "../api/productListTypes";

export function useProducts(parameters: ListProductsParameters) {
  return useQuery({
    queryKey: ["products", "list", parameters],
    queryFn: () => listProducts(parameters),
    staleTime: 30_000,
    placeholderData: keepPreviousData,
  });
}
