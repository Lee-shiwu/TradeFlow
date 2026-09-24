import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { useTaxCategories } from "./useTaxCategories";

const listTaxCategoriesMock = vi.hoisted(() => vi.fn());

vi.mock("../api/listTaxCategories", () => ({
  listTaxCategories: listTaxCategoriesMock,
}));

const parameters = {
  search: "gst",
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
const queryClients = [];

function createWrapper() {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: Infinity } },
  });
  queryClients.push(queryClient);

  return function Wrapper({ children }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  };
}

describe("useTaxCategories", () => {
  beforeEach(() => {
    listTaxCategoriesMock.mockReset();
  });

  afterEach(() => {
    cleanup();
    for (const queryClient of queryClients) {
      queryClient.clear();
    }
    queryClients.length = 0;
  });

  it("is pending while loading", () => {
    listTaxCategoriesMock.mockReturnValue(new Promise(() => {}));
    const { result } = renderHook(() => useTaxCategories(parameters), {
      wrapper: createWrapper(),
    });

    expect(result.current.isPending).toBe(true);
  });

  it("returns tax category data", async () => {
    listTaxCategoriesMock.mockResolvedValue(response);
    const { result } = renderHook(() => useTaxCategories(parameters), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(listTaxCategoriesMock).toHaveBeenCalledWith(parameters);
    expect(result.current.data).toBe(response);
  });

  it("exposes API errors", async () => {
    const error = new ApiError(500, "UNEXPECTED_ERROR", "Tax service failed.");
    listTaxCategoriesMock.mockRejectedValue(error);
    const { result } = renderHook(() => useTaxCategories(parameters), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error).toBe(error);
  });
});
