/** @typedef {import("react").PropsWithChildren} PropsWithChildren */

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { useCreateTaxCategory } from "./useCreateTaxCategory";

const createTaxCategoryMock = vi.hoisted(() => vi.fn());

vi.mock("../api/createTaxCategory", () => ({
  createTaxCategory: createTaxCategoryMock,
}));

const queryClients = [];
const request = {
  code: "REGIONAL",
  name: "Regional levy",
  description: null,
  rate: 0.125,
  treatment: "StandardRated",
  effectiveFrom: "2026-10-01",
  effectiveTo: null,
};
const response = {
  taxCategoryId: "tax-category-id",
  ...request,
  description: "",
  status: "Active",
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

describe("useCreateTaxCategory", () => {
  beforeEach(() => {
    createTaxCategoryMock.mockReset();
  });

  afterEach(() => {
    cleanup();
    for (const queryClient of queryClients) {
      queryClient.clear();
    }
    queryClients.length = 0;
  });

  it("creates a category and updates relevant caches", async () => {
    createTaxCategoryMock.mockResolvedValue(response);
    const { queryClient, Wrapper } = createWrapper();
    const invalidateQueriesSpy = vi.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(() => useCreateTaxCategory(), {
      wrapper: Wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(createTaxCategoryMock).toHaveBeenCalledWith(request);
    expect(
      queryClient.getQueryData([
        "tax-categories",
        "details",
        response.taxCategoryId,
      ]),
    ).toEqual(response);
    expect(invalidateQueriesSpy).toHaveBeenCalledWith({
      queryKey: ["tax-categories", "list"],
    });
  });

  it("exposes an API error", async () => {
    const error = new Error("Create failed");
    createTaxCategoryMock.mockRejectedValue(error);
    const { Wrapper } = createWrapper();
    const { result } = renderHook(() => useCreateTaxCategory(), {
      wrapper: Wrapper,
    });

    act(() => result.current.mutate(request));

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toBe(error);
  });
});
