import { useMutation, useQueryClient } from "@tanstack/react-query";
import { activateProductCategory } from "../api/activateProductCategory";
import { deactivateProductCategory } from "../api/deactivateProductCategory";

/**
 * @typedef {"activate" | "deactivate"} ProductCategoryStatusAction
 */

/**
 * @typedef {Object} ProductCategoryStatusActionParameters
 * @property {ProductCategoryStatusAction} action
 * @property {string} rowVersion
 */

/**
 * @param {string | undefined} productCategoryId
 */
export function useProductCategoryStatusAction(productCategoryId) {
  const queryClient = useQueryClient();

  return useMutation({
    /**
     * @param {ProductCategoryStatusActionParameters} parameters
     */
    mutationFn: ({ action, rowVersion }) => {
      if (!productCategoryId) {
        throw new Error("Product category ID is required.");
      }

      if (action === "activate") {
        return activateProductCategory(productCategoryId, rowVersion);
      }

      if (action === "deactivate") {
        return deactivateProductCategory(productCategoryId, rowVersion);
      }

      throw new Error(`Unsupported product category status action: ${action}`);
    },
    onSuccess: async (response) => {
      queryClient.setQueryData(
        ["product-categories", "details", productCategoryId],
        (currentCategory) => {
          if (!currentCategory) {
            return currentCategory;
          }

          return {
            ...currentCategory,
            status: response.status,
            lastModifiedAt: response.lastModifiedAt,
            lastModifiedBy: response.lastModifiedBy,
            rowVersion: response.rowVersion,
          };
        },
      );

      await queryClient.invalidateQueries({
        queryKey: ["product-categories", "list"],
      });
    },
  });
}
