/** @typedef {import('react').PropsWithChildren} PropsWithChildren */

/** @typedef {import('../api/productDetailsTypes.js').ProductDetails} ProductDetails */

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { useProduct } from "./useProduct";
const getProductByIdMock = vi.hoisted(() => vi.fn());
vi.mock("../api/getProductById", () => ({
  getProductById: getProductByIdMock,
}));
const queryClients = [];
const productId = "33333333-3333-3333-3333-333333333333";
const productDetails = {
  productId,
  sku: "CHAIR-001",
  name: "Office Chair",
  description: "Ergonomic office chair.",
  unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
  productCategoryId: null,
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
  status: "Active",
  createdAt: "2026-08-27T00:00:00+00:00",
  createdBy: "66666666-6666-6666-6666-666666666666",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
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
  queryClients.push(queryClient);
  return function TestQueryClientProvider({ children }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  };
}
describe("useProduct", () => {
  beforeEach(() => {
    getProductByIdMock.mockReset();
  });
  afterEach(() => {
    cleanup();
    for (const queryClient of queryClients) {
      queryClient.clear();
    }
    queryClients.length = 0;
  });
  it("is pending while product details are loading", () => {
    getProductByIdMock.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => useProduct(productId), {
      wrapper: createWrapper(),
    });
    expect(result.current.isPending).toBe(true);
    expect(result.current.isFetching).toBe(true);
    expect(result.current.data).toBeUndefined();
  });
  it("returns the product details when the request succeeds", async () => {
    getProductByIdMock.mockResolvedValue(productDetails);
    const { result } = renderHook(() => useProduct(productId), {
      wrapper: createWrapper(),
    });
    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
    expect(getProductByIdMock).toHaveBeenCalledOnce();
    expect(getProductByIdMock).toHaveBeenCalledWith(productId);
    expect(result.current.data).toEqual(productDetails);
    expect(result.current.isFetching).toBe(false);
  });
  it("returns the API error when the request fails", async () => {
    const error = new ApiError(
      404,
      "PRODUCT_NOT_FOUND",
      "The selected product does not exist.",
      "not-found-trace-id",
    );
    getProductByIdMock.mockRejectedValue(error);
    const { result } = renderHook(() => useProduct(productId), {
      wrapper: createWrapper(),
    });
    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
    expect(result.current.error).toBe(error);
    expect(result.current.data).toBeUndefined();
    expect(getProductByIdMock).toHaveBeenCalledOnce();
  });
  it.each([undefined, ""])(
    "does not automatically request details when product ID is %s",
    (missingProductId) => {
      const { result } = renderHook(() => useProduct(missingProductId), {
        wrapper: createWrapper(),
      });
      expect(getProductByIdMock).not.toHaveBeenCalled();
      expect(result.current.isFetching).toBe(false);
      expect(result.current.data).toBeUndefined();
    },
  );
  it("requests new details when the product ID changes", async () => {
    const secondProductId = "77777777-7777-7777-7777-777777777777";
    const secondProductDetails = {
      ...productDetails,
      productId: secondProductId,
      sku: "DESK-001",
      name: "Office Desk",
      description: "Adjustable office desk.",
    };
    getProductByIdMock
      .mockResolvedValueOnce(productDetails)
      .mockResolvedValueOnce(secondProductDetails);
    const { result, rerender } = renderHook(
      ({ selectedProductId }) => useProduct(selectedProductId),
      {
        initialProps: {
          selectedProductId: productId,
        },
        wrapper: createWrapper(),
      },
    );
    await waitFor(() => {
      expect(result.current.data).toEqual(productDetails);
    });
    rerender({
      selectedProductId: secondProductId,
    });
    await waitFor(() => {
      expect(result.current.data).toEqual(secondProductDetails);
    });
    expect(getProductByIdMock).toHaveBeenCalledTimes(2);
    expect(getProductByIdMock).toHaveBeenNthCalledWith(1, productId);
    expect(getProductByIdMock).toHaveBeenNthCalledWith(2, secondProductId);
  });
});
