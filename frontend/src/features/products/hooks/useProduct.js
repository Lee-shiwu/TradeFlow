import { useQuery } from "@tanstack/react-query";
import { getProductById } from "../api/getProductById";
/**
 * @param {string | undefined} productId
 */
export function useProduct(productId) {
  return useQuery({
    queryKey: ["products", "details", productId],
    queryFn: () => {
      if (!productId) {
        throw new Error("Product ID is required");
      }
      return getProductById(productId);
    },
    enabled: Boolean(productId),
    staleTime: 30_000,
  });
}
