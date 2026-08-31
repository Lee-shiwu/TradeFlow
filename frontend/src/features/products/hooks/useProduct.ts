import { useQuery } from "@tanstack/react-query";
import { getProductById } from "../api/getProductById";

export function useProduct(productId: string | undefined) {
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
