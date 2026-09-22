/** @typedef {import("react").PropsWithChildren} PropsWithChildren */

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { useProductCategories } from "./useProductCategories";

const listProductCategoriesMock = vi.hoisted(() => vi.fn());

vi.mock("../api/listProductCategories", () => ({
  listProductCategories: listProductCategoriesMock,
}));

const parameters = {
  search: "office",
  status: "Active",
  pageNumber: 1,
  pageSize: 20,
};

const response = {
  items: [],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 0,
};

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: Infinity },
    },
  });

  return function TestQueryClientProvider({ children }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  };
}

describe("useProductCategories", () => {
  beforeEach(() => {
    listProductCategoriesMock.mockReset();
  });

  it("is pending while loading", () => {
    listProductCategoriesMock.mockReturnValue(new Promise(() => {}));

    const { result } = renderHook(() => useProductCategories(parameters), {
      wrapper: createWrapper(),
    });

    expect(result.current.isPending).toBe(true);
  });

  it("returns category data", async () => {
    listProductCategoriesMock.mockResolvedValue(response);

    const { result } = renderHook(() => useProductCategories(parameters), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(listProductCategoriesMock).toHaveBeenCalledWith(parameters);
    expect(result.current.data).toBe(response);
  });

  it("returns API errors", async () => {
    const error = new ApiError(500, "UNEXPECTED_ERROR", "Service unavailable.");
    listProductCategoriesMock.mockRejectedValue(error);

    const { result } = renderHook(() => useProductCategories(parameters), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toBe(error);
  });

  it("requests again when parameters change", async () => {
    listProductCategoriesMock.mockResolvedValue(response);

    const { result, rerender } = renderHook(
      (properties) => useProductCategories(properties.parameters),
      {
        initialProps: { parameters },
        wrapper: createWrapper(),
      },
    );

    await waitFor(() => expect(result.current.isSuccess).toBe(true));

    rerender({ parameters: { ...parameters, pageNumber: 2 } });

    await waitFor(() =>
      expect(listProductCategoriesMock).toHaveBeenCalledTimes(2),
    );
  });
});
