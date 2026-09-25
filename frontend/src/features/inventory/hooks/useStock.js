import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { listStock } from "../api/listStock";

export function useStock(parameters) {
  return useQuery({
    queryKey: ["inventory", "stock", parameters],
    queryFn: () => listStock(parameters),
    placeholderData: keepPreviousData,
    staleTime: 30_000,
  });
}
