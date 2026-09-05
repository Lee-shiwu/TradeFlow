import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { useCreateProduct } from "./useCreateProduct";

const createProductMock = vi.hoisted(() => vi.fn());

vi.mock("../api/createProduct", () => ({
  createProduct: createProductMock,
}));

const request = {
  sku: "CHAIR-001",
  name: "Office Chair",
  description: "Ergonomic office chair.",
  unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
};

const response = {
  productId: "33333333-3333-3333-3333-333333333333",
  sku: "CHAIR-001",
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

describe("useCreateProduct", () => {
  beforeEach(() => {
    createProductMock.mockReset();
  });

  afterEach(() => {
    cleanup();

    for (const queryClient of queryClients) {
      queryClient.clear();
    }

    queryClients.length = 0;
  });

  it("calls the create API with the request", async () => {
    createProductMock.mockResolvedValue(response);

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useCreateProduct(), {
      wrapper,
    });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(createProductMock).toHaveBeenCalledOnce();

    expect(createProductMock).toHaveBeenCalledWith(request);
  });

  it("is pending while the create request is running", async () => {
    let resolveRequest;

    createProductMock.mockReturnValue(
      new Promise((resolve) => {
        resolveRequest = resolve;
      }),
    );

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useCreateProduct(), {
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

  it("returns the created product information", async () => {
    createProductMock.mockResolvedValue(response);

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useCreateProduct(), {
      wrapper,
    });

    let createdProduct;

    await act(async () => {
      createdProduct = await result.current.mutateAsync(request);
    });

    expect(createdProduct).toEqual(response);

    expect(createdProduct.productId).toBe(
      "33333333-3333-3333-3333-333333333333",
    );

    expect(createdProduct.sku).toBe("CHAIR-001");
  });

  it("invalidates the product list after success", async () => {
    createProductMock.mockResolvedValue(response);

    const { queryClient, wrapper } = createTestContext();

    const invalidateQueriesSpy = vi.spyOn(queryClient, "invalidateQueries");

    const { result } = renderHook(() => useCreateProduct(), {
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
    [400, "CREATE_PRODUCT_NAME_REQUIRED", "Product name is required."],
    [
      409,
      "CREATE_PRODUCT_SKU_ALREADY_EXISTS",
      "A product with the same SKU already exists.",
    ],
  ])("returns the %i API error", async (status, code, detail) => {
    const apiError = new ApiError(status, code, detail, "test-trace-id");

    createProductMock.mockRejectedValue(apiError);

    const { wrapper } = createTestContext();

    const { result } = renderHook(() => useCreateProduct(), {
      wrapper,
    });

    await act(async () => {
      try {
        await result.current.mutateAsync(request);
      } catch {
        // 通过 Hook 保存的错误状态进行断言。
      }
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBe(apiError);
  });
});
