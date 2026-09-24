import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { useProductCategoryStatusAction } from "./useProductCategoryStatusAction";

const activateProductCategoryMock = vi.hoisted(() => vi.fn());
const deactivateProductCategoryMock = vi.hoisted(() => vi.fn());

vi.mock("../api/activateProductCategory", () => ({
  activateProductCategory: activateProductCategoryMock,
}));

vi.mock("../api/deactivateProductCategory", () => ({
  deactivateProductCategory: deactivateProductCategoryMock,
}));

const productCategoryId = "77777777-7777-7777-7777-777777777777";
const initialRowVersion = "AAAAAAAAB9E=";
const response = {
  productCategoryId,
  status: "Inactive",
  lastModifiedAt: "2026-09-24T01:00:00+00:00",
  lastModifiedBy: "11111111-1111-1111-1111-111111111111",
  rowVersion: "AAAAAAAAB9I=",
};
const category = {
  productCategoryId,
  code: "OFFICE",
  name: "Office products",
  description: "Office items.",
  status: "Active",
  createdAt: "2026-09-20T10:00:00+00:00",
  createdBy: "22222222-2222-2222-2222-222222222222",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: initialRowVersion,
};
const queryClients = [];

function createTestContext() {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false, gcTime: Infinity },
      mutations: { retry: false },
    },
  });
  queryClients.push(queryClient);

  function Wrapper({ children }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  }

  return { queryClient, wrapper: Wrapper };
}

describe("useProductCategoryStatusAction", () => {
  beforeEach(() => {
    activateProductCategoryMock.mockReset();
    deactivateProductCategoryMock.mockReset();
  });

  afterEach(() => {
    cleanup();
    for (const queryClient of queryClients) {
      queryClient.clear();
    }
    queryClients.length = 0;
  });

  it("routes activate and deactivate actions to their APIs", async () => {
    activateProductCategoryMock.mockResolvedValue({
      ...response,
      status: "Active",
    });
    deactivateProductCategoryMock.mockResolvedValue(response);
    const { wrapper } = createTestContext();
    const { result } = renderHook(
      () => useProductCategoryStatusAction(productCategoryId),
      { wrapper },
    );

    await act(async () => {
      await result.current.mutateAsync({
        action: "activate",
        rowVersion: initialRowVersion,
      });
      await result.current.mutateAsync({
        action: "deactivate",
        rowVersion: initialRowVersion,
      });
    });

    expect(activateProductCategoryMock).toHaveBeenCalledWith(
      productCategoryId,
      initialRowVersion,
    );
    expect(deactivateProductCategoryMock).toHaveBeenCalledWith(
      productCategoryId,
      initialRowVersion,
    );
  });

  it("updates details and invalidates lists after success", async () => {
    deactivateProductCategoryMock.mockResolvedValue(response);
    const { queryClient, wrapper } = createTestContext();
    queryClient.setQueryData(
      ["product-categories", "details", productCategoryId],
      category,
    );
    const invalidateQueriesSpy = vi.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(
      () => useProductCategoryStatusAction(productCategoryId),
      { wrapper },
    );

    await act(async () => {
      await result.current.mutateAsync({
        action: "deactivate",
        rowVersion: initialRowVersion,
      });
    });

    expect(
      queryClient.getQueryData([
        "product-categories",
        "details",
        productCategoryId,
      ]),
    ).toEqual({
      ...category,
      status: "Inactive",
      lastModifiedAt: response.lastModifiedAt,
      lastModifiedBy: response.lastModifiedBy,
      rowVersion: response.rowVersion,
    });
    expect(invalidateQueriesSpy).toHaveBeenCalledWith({
      queryKey: ["product-categories", "list"],
    });
  });

  it("exposes pending and error states", async () => {
    let rejectRequest;
    deactivateProductCategoryMock.mockReturnValue(
      new Promise((_, reject) => {
        rejectRequest = reject;
      }),
    );
    const { wrapper } = createTestContext();
    const { result } = renderHook(
      () => useProductCategoryStatusAction(productCategoryId),
      { wrapper },
    );

    act(() => {
      result.current.mutate({
        action: "deactivate",
        rowVersion: initialRowVersion,
      });
    });
    await waitFor(() => expect(result.current.isPending).toBe(true));

    act(() => rejectRequest(new Error("Status update failed")));
    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.error.message).toBe("Status update failed");
  });

  it("rejects unsupported actions", async () => {
    const { wrapper } = createTestContext();
    const { result } = renderHook(
      () => useProductCategoryStatusAction(productCategoryId),
      { wrapper },
    );

    await act(async () => {
      await expect(
        result.current.mutateAsync({
          action: "remove",
          rowVersion: initialRowVersion,
        }),
      ).rejects.toThrow("Unsupported product category status action: remove");
    });
  });
});
