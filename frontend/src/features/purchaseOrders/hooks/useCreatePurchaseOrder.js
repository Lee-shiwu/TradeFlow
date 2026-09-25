import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createPurchaseOrder } from "../api/createPurchaseOrder";

export function useCreatePurchaseOrder() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: createPurchaseOrder,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["purchase-orders", "list"],
      });
    },
  });
}
