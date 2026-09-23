import { useQuery } from "@tanstack/react-query";
import { getProductCategoryById } from "../api/getProductCategoryById";

/**
 * @param {string | undefined} productCategoryId
 */
export function useProductCategory(productCategoryId) {
  return useQuery({
    queryKey: ["product-categories", "details", productCategoryId],
    queryFn: () => {
      if (!productCategoryId) {
        throw new Error("Product category ID is required.");
      }

      return getProductCategoryById(productCategoryId);
    },
    enabled: Boolean(productCategoryId),
    staleTime: 30_000,
  });
}
