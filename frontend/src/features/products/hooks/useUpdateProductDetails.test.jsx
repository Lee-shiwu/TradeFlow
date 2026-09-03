import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { useUpdateProductDetails } from "./useUpdateProductDetails";

const updateProductDetailsMock = vi.hoisted(() => vi.fn());

vi.mock("../api/updateProductDetails", () => ({
  updateProductDetails: updateProductDetailsMock,
}));

const productId = "33333333-3333-3333-3333-333333333333";

const request = {
  name: "Updated Office Chair",
  description: "Updated ergonomic office chair.",
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
  rowVersion: "AAAAAAAAB9E=",
};

const response = {
  productId,
  name: "Updated Office Chair",
  description: "Updated ergonomic office chair.",
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
  lastModifiedAt: "2026-09-03T01:30:00+00:00",
  lastModifiedBy: "88888888-8888-8888-8888-888888888888",
  rowVersion: "AAAAAAAAB9F=",
};

const currentProduct = {
  productId,
  sku: "CHAIR-001",
  name: "Office Chair",
  description: "Ergonomic office chair.",
  unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
  productCategoryId: null,
  taxCategoryId: "99999999-9999-9999-9999-999999999999",
  status: "Active",
  createdAt: "2026-08-27T00:00:00+00:00",
  createdBy: "66666666-6666-6666-6666-666666666666",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

const queryClients = [];

function createTestContext() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: Infinity,
      },
      mutations: {
        retry: false,
      },
    },
  });

  queryClients.push(queryClient);

  function TestQueryClientProvider({ children }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  }

  return {
    queryClient,
    wrapper: TestQueryClientProvider,
  };
}

describe("useUpdateProductDetails", () => {
  beforeEach(() => {
    updateProductDetailsMock.mockReset();
  });

  afterEach(() => {
    cleanup();

    for (const queryClient of queryClients) {
      queryClient.clear();
    }

    queryClients.length = 0;
  });

  it("calls the update API with the product ID and request", async () => {
    updateProductDetailsMock.mockResolvedValue(response);

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useUpdateProductDetails(productId), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(updateProductDetailsMock).toHaveBeenCalledOnce();

    expect(updateProductDetailsMock).toHaveBeenCalledWith(productId, request);
  });

  it("is pending while the update request is running", async () => {
    let resolveRequest;

    updateProductDetailsMock.mockReturnValue(
      new Promise((resolve) => {
        resolveRequest = resolve;
      }),
    );

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useUpdateProductDetails(productId), {
      wrapper,
    });

    act(() => {
      result.current.mutate(request);
    });

    await waitFor(() => {
      expect(result.current.isPending).toBe(true);
    });

    await act(async () => {
      resolveRequest(response);
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
  });

  it("updates the product details cache after success", async () => {
    updateProductDetailsMock.mockResolvedValue(response);

    const { queryClient, wrapper } = createTestContext();

    queryClient.setQueryData(
      ["products", "details", productId],
      currentProduct,
    );

    const { result } = renderHook(() => useUpdateProductDetails(productId), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    const cachedProduct = queryClient.getQueryData([
      "products",
      "details",
      productId,
    ]);

    expect(cachedProduct).toEqual({
      ...currentProduct,
      name: response.name,
      description: response.description,
      productCategoryId: response.productCategoryId,
      taxCategoryId: response.taxCategoryId,
      lastModifiedAt: response.lastModifiedAt,
      lastModifiedBy: response.lastModifiedBy,
      rowVersion: response.rowVersion,
    });
  });

  it("preserves fields that cannot be updated", async () => {
    updateProductDetailsMock.mockResolvedValue(response);

    const { queryClient, wrapper } = createTestContext();

    queryClient.setQueryData(
      ["products", "details", productId],
      currentProduct,
    );

    const { result } = renderHook(() => useUpdateProductDetails(productId), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    const cachedProduct = queryClient.getQueryData([
      "products",
      "details",
      productId,
    ]);

    expect(cachedProduct.sku).toBe(currentProduct.sku);

    expect(cachedProduct.unitOfMeasureId).toBe(currentProduct.unitOfMeasureId);

    expect(cachedProduct.status).toBe(currentProduct.status);

    expect(cachedProduct.createdAt).toBe(currentProduct.createdAt);

    expect(cachedProduct.createdBy).toBe(currentProduct.createdBy);
  });

  it("invalidates the product list after success", async () => {
    updateProductDetailsMock.mockResolvedValue(response);

    const { queryClient, wrapper } = createTestContext();

    const invalidateQueriesSpy = vi.spyOn(queryClient, "invalidateQueries");

    const { result } = renderHook(() => useUpdateProductDetails(productId), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(invalidateQueriesSpy).toHaveBeenCalledWith({
      queryKey: ["products", "list"],
    });
  });

  it.each([
    [400, "UPDATE_PRODUCT_NAME_REQUIRED", "Product name is required."],
    [404, "PRODUCT_NOT_FOUND", "The selected product does not exist."],
    [
      409,
      "PRODUCT_CONCURRENCY_CONFLICT",
      "The product was changed by another request.",
    ],
  ])("returns the %i API error", async (status, code, detail) => {
    const apiError = new ApiError(status, code, detail, "test-trace-id");

    updateProductDetailsMock.mockRejectedValue(apiError);

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useUpdateProductDetails(productId), {
      wrapper,
    });

    await act(async () => {
      try {
        await result.current.mutateAsync(request);
      } catch {
        // 通过 Hook 的错误状态进行断言。
      }
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBe(apiError);
  });
});
