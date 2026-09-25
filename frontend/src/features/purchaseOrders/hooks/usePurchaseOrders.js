import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { listPurchaseOrders } from "../api/listPurchaseOrders";

export function usePurchaseOrders(parameters) {
  return useQuery({
    queryKey: ["purchase-orders", "list", parameters],
    queryFn: () => listPurchaseOrders(parameters),
    staleTime: 30_000,
    placeholderData: keepPreviousData,
  });
}
