/** @typedef {import("react").PropsWithChildren} PropsWithChildren */

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { useProductCategory } from "./useProductCategory";

const getProductCategoryByIdMock = vi.hoisted(() => vi.fn());

vi.mock("../api/getProductCategoryById", () => ({
  getProductCategoryById: getProductCategoryByIdMock,
}));

const queryClients = [];
const productCategoryId = "77777777-7777-7777-7777-777777777777";

const details = {
  productCategoryId,
  code: "OFFICE",
  name: "Office products",
  description: "Products used in the office.",
  status: "Active",
  createdAt: "2026-09-20T10:00:00+00:00",
  createdBy: "11111111-1111-1111-1111-111111111111",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: Infinity },
    },
  });
  queryClients.push(queryClient);

  return function TestQueryClientProvider({ children }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  };
}

describe("useProductCategory", () => {
  beforeEach(() => {
    getProductCategoryByIdMock.mockReset();
  });

  afterEach(() => {
    cleanup();
    for (const queryClient of queryClients) {
      queryClient.clear();
    }
    queryClients.length = 0;
  });

  it("is pending while details are loading", () => {
    getProductCategoryByIdMock.mockReturnValue(new Promise(() => {}));

    const { result } = renderHook(() => useProductCategory(productCategoryId), {
      wrapper: createWrapper(),
    });

    expect(result.current.isPending).toBe(true);
    expect(result.current.isFetching).toBe(true);
  });

  it("returns category details", async () => {
    getProductCategoryByIdMock.mockResolvedValue(details);

    const { result } = renderHook(() => useProductCategory(productCategoryId), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(getProductCategoryByIdMock).toHaveBeenCalledWith(productCategoryId);
    expect(result.current.data).toEqual(details);
  });

  it("returns an API error", async () => {
    const error = new ApiError(
      404,
      "PRODUCT_CATEGORY_NOT_FOUND",
      "The requested product category was not found.",
    );
    getProductCategoryByIdMock.mockRejectedValue(error);

    const { result } = renderHook(() => useProductCategory(productCategoryId), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBe(error);
  });

  it.each([undefined, ""])(
    "does not request details when category ID is %s",
    (missingProductCategoryId) => {
      const { result } = renderHook(
        () => useProductCategory(missingProductCategoryId),
        { wrapper: createWrapper() },
      );

      expect(getProductCategoryByIdMock).not.toHaveBeenCalled();
      expect(result.current.isFetching).toBe(false);
    },
  );
});
