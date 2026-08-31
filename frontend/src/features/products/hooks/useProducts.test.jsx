/** @typedef {import('react').PropsWithChildren} PropsWithChildren */

/** @typedef {import('../api/productListTypes.js').ListProductsParameters} ListProductsParameters */

/** @typedef {import('../api/productListTypes.js').ListProductsResponse} ListProductsResponse */

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { useProducts } from "./useProducts";
const listProductsMock = vi.hoisted(() => vi.fn());
vi.mock("../api/listProducts", () => ({
  listProducts: listProductsMock,
}));
const parameters = {
  search: "chair",
  status: "Active",
  pageNumber: 1,
  pageSize: 20,
};
const response = {
  items: [
    {
      productId: "33333333-3333-3333-3333-333333333333",
      sku: "CHAIR-001",
      name: "Office Chair",
      unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
      productCategoryId: null,
      taxCategoryId: "55555555-5555-5555-5555-555555555555",
      status: "Active",
      createdAt: "2026-08-27T00:00:00+00:00",
      lastModifiedAt: null,
    },
  ],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 1,
  totalPages: 1,
};
function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: Infinity,
      },
    },
  });
  return function TestQueryClientProvider({ children }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  };
}
describe("useProducts", () => {
  beforeEach(() => {
    listProductsMock.mockReset();
  });
  it("is pending while the product request is running", () => {
    listProductsMock.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => useProducts(parameters), {
      wrapper: createWrapper(),
    });
    expect(result.current.isPending).toBe(true);
  });
  it("returns the product list response", async () => {
    listProductsMock.mockResolvedValue(response);
    const { result } = renderHook(() => useProducts(parameters), {
      wrapper: createWrapper(),
    });
    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
    expect(listProductsMock).toHaveBeenCalledOnce();
    expect(listProductsMock).toHaveBeenCalledWith(parameters);
    expect(result.current.isPending).toBe(false);
    expect(result.current.data).toEqual(response);
  });
  it("returns the API error when the request fails", async () => {
    const apiError = new ApiError(
      500,
      "UNEXPECTED_ERROR",
      "An unexpected error occurred.",
      "test-trace-id",
    );
    listProductsMock.mockRejectedValue(apiError);
    const { result } = renderHook(() => useProducts(parameters), {
      wrapper: createWrapper(),
    });
    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
    expect(result.current.error).toBe(apiError);
    expect(result.current.data).toBeUndefined();
  });
  it("requests new data when query parameters change", async () => {
    listProductsMock.mockResolvedValue(response);
    const secondPageParameters = {
      ...parameters,
      pageNumber: 2,
    };
    const { result, rerender } = renderHook(
      (properties) => useProducts(properties.parameters),
      {
        initialProps: {
          parameters,
        },
        wrapper: createWrapper(),
      },
    );
    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
    rerender({
      parameters: secondPageParameters,
    });
    await waitFor(() => {
      expect(listProductsMock).toHaveBeenCalledTimes(2);
    });
    expect(listProductsMock).toHaveBeenNthCalledWith(1, parameters);
    expect(listProductsMock).toHaveBeenNthCalledWith(2, secondPageParameters);
  });
});
