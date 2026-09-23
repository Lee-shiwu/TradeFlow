/** @typedef {import("react").PropsWithChildren} PropsWithChildren */

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { useUpdateProductCategoryDetails } from "./useUpdateProductCategoryDetails";

const updateProductCategoryDetailsMock = vi.hoisted(() => vi.fn());

vi.mock("../api/updateProductCategoryDetails", () => ({
  updateProductCategoryDetails: updateProductCategoryDetailsMock,
}));

const productCategoryId = "77777777-7777-7777-7777-777777777777";
const request = {
  name: "Updated category",
  description: null,
  rowVersion: "AAAAAAAAB9E=",
};
const response = {
  productCategoryId,
  code: "OFFICE",
  name: request.name,
  description: "",
  status: "Active",
  createdAt: "2026-09-20T10:00:00+00:00",
  createdBy: "11111111-1111-1111-1111-111111111111",
  lastModifiedAt: "2026-09-23T10:30:00+00:00",
  lastModifiedBy: "22222222-2222-2222-2222-222222222222",
  rowVersion: "AAAAAAAAB9I=",
};

const queryClients = [];

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

describe("useUpdateProductCategoryDetails", () => {
  beforeEach(() => {
    updateProductCategoryDetailsMock.mockReset();
  });

  afterEach(() => {
    cleanup();
    for (const queryClient of queryClients) {
      queryClient.clear();
    }
    queryClients.length = 0;
  });

  it("updates the detail cache and invalidates category lists", async () => {
    updateProductCategoryDetailsMock.mockResolvedValue(response);
    const { queryClient, Wrapper } = createWrapper();
    const invalidateQueriesSpy = vi.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(
      () => useUpdateProductCategoryDetails(productCategoryId),
      { wrapper: Wrapper },
    );

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(updateProductCategoryDetailsMock).toHaveBeenCalledWith(
      productCategoryId,
      request,
    );
    expect(
      queryClient.getQueryData([
        "product-categories",
        "details",
        productCategoryId,
      ]),
    ).toEqual(response);
    expect(invalidateQueriesSpy).toHaveBeenCalledWith({
      queryKey: ["product-categories", "list"],
    });
  });

  it("exposes update errors", async () => {
    const error = new Error("Update failed");
    updateProductCategoryDetailsMock.mockRejectedValue(error);
    const { Wrapper } = createWrapper();
    const { result } = renderHook(
      () => useUpdateProductCategoryDetails(productCategoryId),
      { wrapper: Wrapper },
    );

    act(() => {
      result.current.mutate(request);
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toBe(error);
  });
});
