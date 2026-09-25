/** @typedef {import("../api/supplierTypes.js").ListSuppliersParameters} ListSuppliersParameters */

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { listSuppliers } from "../api/listSuppliers";

/**
 * @param {ListSuppliersParameters} parameters
 */
export function useSuppliers(parameters) {
  return useQuery({
    queryKey: ["suppliers", "list", parameters],
    queryFn: () => listSuppliers(parameters),
    staleTime: 30_000,
    placeholderData: keepPreviousData,
  });
}
