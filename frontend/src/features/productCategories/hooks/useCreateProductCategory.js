/** @typedef {import("../api/createProductCategoryTypes.js").CreateProductCategoryRequest} CreateProductCategoryRequest */

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createProductCategory } from "../api/createProductCategory";

export function useCreateProductCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    /**
     * @param {CreateProductCategoryRequest} request
     */
    mutationFn: (request) => createProductCategory(request),
    onSuccess: async (createdCategory) => {
      queryClient.setQueryData(
        ["product-categories", "details", createdCategory.productCategoryId],
        createdCategory,
      );

      await queryClient.invalidateQueries({
        queryKey: ["product-categories", "list"],
      });
    },
  });
}
