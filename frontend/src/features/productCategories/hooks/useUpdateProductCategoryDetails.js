/** @typedef {import("../api/updateProductCategoryDetailsTypes.js").UpdateProductCategoryDetailsRequest} UpdateProductCategoryDetailsRequest */

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateProductCategoryDetails } from "../api/updateProductCategoryDetails";

/**
 * @param {string | undefined} productCategoryId
 */
export function useUpdateProductCategoryDetails(productCategoryId) {
  const queryClient = useQueryClient();

  return useMutation({
    /**
     * @param {UpdateProductCategoryDetailsRequest} request
     */
    mutationFn: (request) => {
      if (!productCategoryId) {
        throw new Error("Product category ID is required.");
      }

      return updateProductCategoryDetails(productCategoryId, request);
    },
    onSuccess: async (updatedCategory) => {
      queryClient.setQueryData(
        ["product-categories", "details", productCategoryId],
        updatedCategory,
      );

      await queryClient.invalidateQueries({
        queryKey: ["product-categories", "list"],
      });
    },
  });
}
