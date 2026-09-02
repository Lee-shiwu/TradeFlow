import { useMutation, useQueryClient } from "@tanstack/react-query";
import { activateProduct } from "../api/activateProduct";
import { deactivateProduct } from "../api/deactivateProduct";

/**
 * @typedef {"activate" | "deactivate"} ProductStatusAction
 */

/**
 * @typedef {Object} ProductStatusActionParameters
 * @property {ProductStatusAction} action
 * @property {string} rowVersion
 */

/**
 * 管理商品启用和停用操作。
 *
 * @param {string} productId
 */
export function useProductStatusAction(productId) {
  const queryClient = useQueryClient();

  return useMutation({
    /**
     * @param {ProductStatusActionParameters} parameters
     */
    mutationFn: ({ action, rowVersion }) => {
      if (action === "activate") {
        return activateProduct(productId, rowVersion);
      }

      if (action === "deactivate") {
        return deactivateProduct(productId, rowVersion);
      }

      throw new Error(`Unsupported product status action: ${action}`);
    },

    onSuccess: async (response) => {
      queryClient.setQueryData(
        ["products", "details", productId],
        (currentProduct) => {
          if (!currentProduct) {
            return currentProduct;
          }

          return {
            ...currentProduct,
            status: response.status,
            lastModifiedAt: response.lastModifiedAt,
            lastModifiedBy: response.lastModifiedBy,
            rowVersion: response.rowVersion,
          };
        },
      );

      await queryClient.invalidateQueries({
        queryKey: ["products", "list"],
      });
    },
  });
}
