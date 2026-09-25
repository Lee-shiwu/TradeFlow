/** @typedef {import("../api/createTaxCategoryTypes.js").CreateTaxCategoryRequest} CreateTaxCategoryRequest */

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createTaxCategory } from "../api/createTaxCategory";

export function useCreateTaxCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    /**
     * @param {CreateTaxCategoryRequest} request
     */
    mutationFn: (request) => createTaxCategory(request),
    onSuccess: async (createdCategory) => {
      queryClient.setQueryData(
        ["tax-categories", "details", createdCategory.taxCategoryId],
        createdCategory,
      );

      await queryClient.invalidateQueries({
        queryKey: ["tax-categories", "list"],
      });
    },
  });
}
