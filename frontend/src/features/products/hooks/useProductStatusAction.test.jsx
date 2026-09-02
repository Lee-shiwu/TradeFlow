import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { useProductStatusAction } from "./useProductStatusAction";

const activateProductMock = vi.hoisted(() => vi.fn());
const deactivateProductMock = vi.hoisted(() => vi.fn());

vi.mock("../api/activateProduct", () => ({
  activateProduct: activateProductMock,
}));

vi.mock("../api/deactivateProduct", () => ({
  deactivateProduct: deactivateProductMock,
}));

const productId = "33333333-3333-3333-3333-333333333333";

const initialRowVersion = "AAAAAAAAB9E=";
const updatedRowVersion = "AAAAAAAAB9F=";

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
  rowVersion: initialRowVersion,
};

const activatedResponse = {
  productId,
  status: "Active",
  lastModifiedAt: "2026-09-01T01:00:00+00:00",
  lastModifiedBy: "77777777-7777-7777-7777-777777777777",
  rowVersion: updatedRowVersion,
};

const deactivatedResponse = {
  ...activatedResponse,
  status: "Inactive",
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

describe("useProductStatusAction", () => {
  beforeEach(() => {
    activateProductMock.mockReset();
    deactivateProductMock.mockReset();
  });

  afterEach(() => {
    cleanup();

    for (const queryClient of queryClients) {
      queryClient.clear();
    }

    queryClients.length = 0;
  });

  it("calls the activate API for an activate action", async () => {
    activateProductMock.mockResolvedValue(activatedResponse);

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useProductStatusAction(productId), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync({
        action: "activate",
        rowVersion: initialRowVersion,
      });
    });

    expect(activateProductMock).toHaveBeenCalledOnce();

    expect(activateProductMock).toHaveBeenCalledWith(
      productId,
      initialRowVersion,
    );

    expect(deactivateProductMock).not.toHaveBeenCalled();
  });

  it("calls the deactivate API for a deactivate action", async () => {
    deactivateProductMock.mockResolvedValue(deactivatedResponse);

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useProductStatusAction(productId), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync({
        action: "deactivate",
        rowVersion: initialRowVersion,
      });
    });

    expect(deactivateProductMock).toHaveBeenCalledOnce();

    expect(deactivateProductMock).toHaveBeenCalledWith(
      productId,
      initialRowVersion,
    );

    expect(activateProductMock).not.toHaveBeenCalled();
  });

  it("is pending while the request is running", async () => {
    let resolveRequest;

    deactivateProductMock.mockReturnValue(
      new Promise((resolve) => {
        resolveRequest = resolve;
      }),
    );

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useProductStatusAction(productId), {
      wrapper,
    });

    act(() => {
      result.current.mutate({
        action: "deactivate",
        rowVersion: initialRowVersion,
      });
    });

    await waitFor(() => {
      expect(result.current.isPending).toBe(true);
    });

    await act(async () => {
      resolveRequest(deactivatedResponse);
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });
  });

  it("updates the product details cache after success", async () => {
    deactivateProductMock.mockResolvedValue(deactivatedResponse);

    const { queryClient, wrapper } = createTestContext();

    queryClient.setQueryData(
      ["products", "details", productId],
      productDetails,
    );

    const { result } = renderHook(() => useProductStatusAction(productId), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync({
        action: "deactivate",
        rowVersion: initialRowVersion,
      });
    });

    const cachedProduct = queryClient.getQueryData([
      "products",
      "details",
      productId,
    ]);

    expect(cachedProduct).toEqual({
      ...productDetails,
      status: "Inactive",
      lastModifiedAt: deactivatedResponse.lastModifiedAt,
      lastModifiedBy: deactivatedResponse.lastModifiedBy,
      rowVersion: updatedRowVersion,
    });
  });

  it("invalidates the product list after success", async () => {
    activateProductMock.mockResolvedValue(activatedResponse);

    const { queryClient, wrapper } = createTestContext();

    const invalidateQueriesSpy = vi.spyOn(queryClient, "invalidateQueries");

    const { result } = renderHook(() => useProductStatusAction(productId), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync({
        action: "activate",
        rowVersion: initialRowVersion,
      });
    });

    expect(invalidateQueriesSpy).toHaveBeenCalledWith({
      queryKey: ["products", "list"],
    });
  });

  it("returns the API error when the request fails", async () => {
    const apiError = new ApiError(
      409,
      "PRODUCT_CONCURRENCY_CONFLICT",
      "The product was changed by another request.",
      "concurrency-trace-id",
    );

    deactivateProductMock.mockRejectedValue(apiError);

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useProductStatusAction(productId), {
      wrapper,
    });

    await act(async () => {
      try {
        await result.current.mutateAsync({
          action: "deactivate",
          rowVersion: initialRowVersion,
        });
      } catch {
        // 通过 Hook 的 error 状态检查错误。
      }
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBe(apiError);
  });

  it("rejects unsupported actions", async () => {
    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useProductStatusAction(productId), {
      wrapper,
    });

    await act(async () => {
      try {
        await result.current.mutateAsync({
          action: "remove",
          rowVersion: initialRowVersion,
        });
      } catch {
        // 通过 Hook 的 error 状态检查错误。
      }
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error.message).toBe(
      "Unsupported product status action: remove",
    );

    expect(activateProductMock).not.toHaveBeenCalled();
    expect(deactivateProductMock).not.toHaveBeenCalled();
  });
});
