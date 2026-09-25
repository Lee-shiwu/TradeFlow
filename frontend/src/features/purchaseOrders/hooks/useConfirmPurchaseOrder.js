import { useMutation, useQueryClient } from "@tanstack/react-query";
import { confirmPurchaseOrder } from "../api/confirmPurchaseOrder";

export function useConfirmPurchaseOrder() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ purchaseOrderId, rowVersion }) =>
      confirmPurchaseOrder(purchaseOrderId, rowVersion),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["purchase-orders"] });
    },
  });
}
