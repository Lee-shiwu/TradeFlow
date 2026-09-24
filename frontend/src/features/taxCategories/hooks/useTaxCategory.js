import { useQuery } from "@tanstack/react-query";
import { getTaxCategoryById } from "../api/getTaxCategoryById";

/**
 * @param {string | undefined} taxCategoryId
 */
export function useTaxCategory(taxCategoryId) {
  return useQuery({
    queryKey: ["tax-categories", "details", taxCategoryId],
    queryFn: () => {
      if (!taxCategoryId) {
        throw new Error("Tax category ID is required.");
      }

      return getTaxCategoryById(taxCategoryId);
    },
    enabled: Boolean(taxCategoryId),
    staleTime: 30_000,
  });
}
