/** @typedef {import ('../api/createProductTypes').CreateProductRequest} CreateProductRequest */
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createProduct } from "../api/createProduct";

/**
 * 管理新建商品操作
 */
export function useCreateProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    /**
     * @param {CreateProductRequest} request
     */
    mutationFn: (request) => {
      return createProduct(request);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["products", "list"],
      });
    },
  });
}
