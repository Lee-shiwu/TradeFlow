/** @typedef {import("react").PropsWithChildren} PropsWithChildren */

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { useCreateProductCategory } from "./useCreateProductCategory";

const createProductCategoryMock = vi.hoisted(() => vi.fn());

vi.mock("../api/createProductCategory", () => ({
  createProductCategory: createProductCategoryMock,
}));

const queryClients = [];

const request = {
  code: "OFFICE",
  name: "Office products",
  description: null,
};

const response = {
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  ...request,
  description: "",
  status: "Active",
  createdAt: "2026-09-23T08:30:00+00:00",
  createdBy: "11111111-1111-1111-1111-111111111111",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      mutations: { retry: false },
      queries: { retry: false, gcTime: Infinity },
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

describe("useCreateProductCategory", () => {
  beforeEach(() => {
    createProductCategoryMock.mockReset();
  });

  afterEach(() => {
    cleanup();
    for (const queryClient of queryClients) {
      queryClient.clear();
    }
    queryClients.length = 0;
  });

  it("creates a category and updates relevant caches", async () => {
    createProductCategoryMock.mockResolvedValue(response);
    const { queryClient, Wrapper } = createWrapper();
    const invalidateQueriesSpy = vi.spyOn(queryClient, "invalidateQueries");

    const { result } = renderHook(() => useCreateProductCategory(), {
      wrapper: Wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(createProductCategoryMock).toHaveBeenCalledWith(request);
    expect(
      queryClient.getQueryData([
        "product-categories",
        "details",
        response.productCategoryId,
      ]),
    ).toEqual(response);
    expect(invalidateQueriesSpy).toHaveBeenCalledWith({
      queryKey: ["product-categories", "list"],
    });
  });

  it("exposes an API error", async () => {
    const error = new Error("Create failed");
    createProductCategoryMock.mockRejectedValue(error);
    const { Wrapper } = createWrapper();
    const { result } = renderHook(() => useCreateProductCategory(), {
      wrapper: Wrapper,
    });

    act(() => {
      result.current.mutate(request);
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBe(error);
  });
});
