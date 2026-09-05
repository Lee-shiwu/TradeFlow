import { useQuery } from "@tanstack/react-query";
import { getProductReferenceData } from "../api/getProductReferenceData";

/**
 * @param {boolean} enabled
 */
export function useProductReferenceData(enabled = true) {
  return useQuery({
    queryKey: ["products", "reference-data"],
    queryFn: getProductReferenceData,
    enabled,
    staleTime: 5 * 60_000,
  });
}
