import { useMutation, useQueryClient } from "@tanstack/react-query";
import { receivePurchaseOrder } from "../api/receivePurchaseOrder";

export function useReceivePurchaseOrder() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ purchaseOrderId, rowVersion }) =>
      receivePurchaseOrder(purchaseOrderId, rowVersion),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["purchase-orders"] }),
        queryClient.invalidateQueries({ queryKey: ["inventory", "stock"] }),
      ]);
    },
  });
}
