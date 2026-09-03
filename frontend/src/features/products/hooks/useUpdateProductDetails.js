/**
 * @typedef {import("../api/updateProductDetailsTypes.js").UpdateProductDetailsRequest}
 * UpdateProductDetailsRequest
 */

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateProductDetails } from "../api/updateProductDetails";

/**
 * 管理商品详情修改操作。
 *
 * @param {string} productId
 */
export function useUpdateProductDetails(productId) {
  const queryClient = useQueryClient();

  return useMutation({
    /**
     * @param {UpdateProductDetailsRequest} request
     */
    mutationFn: (request) => {
      return updateProductDetails(productId, request);
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
            name: response.name,
            description: response.description,
            productCategoryId: response.productCategoryId,
            taxCategoryId: response.taxCategoryId,
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
