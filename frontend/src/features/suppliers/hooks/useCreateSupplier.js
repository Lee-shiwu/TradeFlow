/** @typedef {import("../api/supplierTypes.js").CreateSupplierRequest} CreateSupplierRequest */

import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createSupplier } from "../api/createSupplier";

export function useCreateSupplier() {
  const queryClient = useQueryClient();

  return useMutation({
    /**
     * @param {CreateSupplierRequest} request
     */
    mutationFn: (request) => createSupplier(request),
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["suppliers", "list"],
      });
    },
  });
}
