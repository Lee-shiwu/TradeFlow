/** @typedef {import("react").PropsWithChildren} PropsWithChildren */

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { createSupplier } from "../api/createSupplier";
import { listSuppliers } from "../api/listSuppliers";
import { useCreateSupplier } from "./useCreateSupplier";
import { useSuppliers } from "./useSuppliers";

vi.mock("../api/createSupplier", () => ({ createSupplier: vi.fn() }));
vi.mock("../api/listSuppliers", () => ({ listSuppliers: vi.fn() }));

const queryClients = [];

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: Infinity },
      mutations: { retry: false },
    },
  });
  queryClients.push(queryClient);

  return {
    queryClient,
    Wrapper: function TestQueryClientProvider({ children }) {
      return (
        <QueryClientProvider client={queryClient}>
          {children}
        </QueryClientProvider>
      );
    },
  };
}

describe("supplier hooks", () => {
  beforeEach(() => {
    vi.mocked(createSupplier).mockReset();
    vi.mocked(listSuppliers).mockReset();
  });

  afterEach(() => {
    cleanup();
    for (const queryClient of queryClients) {
      queryClient.clear();
    }
    queryClients.length = 0;
  });

  it("loads suppliers with the selected parameters", async () => {
    const parameters = {
      search: "office",
      status: "Active",
      pageNumber: 1,
      pageSize: 20,
    };
    vi.mocked(listSuppliers).mockResolvedValue({ items: [] });
    const { Wrapper } = createWrapper();
    const { result } = renderHook(() => useSuppliers(parameters), {
      wrapper: Wrapper,
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(listSuppliers).toHaveBeenCalledWith(parameters);
  });

  it("creates a supplier and invalidates supplier lists", async () => {
    const request = { code: "OFFICE", name: "Office supplier" };
    vi.mocked(createSupplier).mockResolvedValue({ supplierId: "supplier-id" });
    const { queryClient, Wrapper } = createWrapper();
    const invalidateQueriesSpy = vi.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(() => useCreateSupplier(), {
      wrapper: Wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(createSupplier).toHaveBeenCalledWith(request);
    expect(invalidateQueriesSpy).toHaveBeenCalledWith({
      queryKey: ["suppliers", "list"],
    });
  });
});
