import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { cleanup, renderHook, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { useProductReferenceData } from "./useProductReferenceData";

const getProductReferenceDataMock = vi.hoisted(() => vi.fn());

vi.mock("../api/getProductReferenceData", () => ({
  getProductReferenceData: getProductReferenceDataMock,
}));

const referenceData = {
  unitsOfMeasure: [
    {
      id: "10000000-0000-0000-0000-000000000001",
      code: "EA",
      name: "Each",
    },
  ],
  productCategories: [],
  taxCategories: [
    {
      id: "20000000-0000-0000-0000-000000000001",
      code: "GST15",
      name: "Standard GST",
    },
  ],
};

const queryClients = [];

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

describe("useProductReferenceData", () => {
  beforeEach(() => {
    getProductReferenceDataMock.mockReset();
  });

  afterEach(() => {
    cleanup();

    for (const queryClient of queryClients) {
      queryClient.clear();
    }

    queryClients.length = 0;
  });

  it("loads product reference data", async () => {
    getProductReferenceDataMock.mockResolvedValue(referenceData);

    const { result } = renderHook(() => useProductReferenceData(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(getProductReferenceDataMock).toHaveBeenCalledOnce();
    expect(result.current.data).toEqual(referenceData);
  });

  it("does not load reference data while disabled", () => {
    const { result } = renderHook(() => useProductReferenceData(false), {
      wrapper: createWrapper(),
    });

    expect(getProductReferenceDataMock).not.toHaveBeenCalled();
    expect(result.current.isFetching).toBe(false);
    expect(result.current.data).toBeUndefined();
  });

  it("loads reference data when enabled later", async () => {
    getProductReferenceDataMock.mockResolvedValue(referenceData);

    const { result, rerender } = renderHook(
      ({ enabled }) => useProductReferenceData(enabled),
      {
        initialProps: {
          enabled: false,
        },
        wrapper: createWrapper(),
      },
    );

    rerender({
      enabled: true,
    });

    await waitFor(() => {
      expect(result.current.isSuccess).toBe(true);
    });

    expect(getProductReferenceDataMock).toHaveBeenCalledOnce();
  });

  it("returns an API error", async () => {
    const error = new ApiError(
      500,
      "UNEXPECTED_ERROR",
      "Unable to load product reference data.",
      "reference-data-trace-id",
    );

    getProductReferenceDataMock.mockRejectedValue(error);

    const { result } = renderHook(() => useProductReferenceData(), {
      wrapper: createWrapper(),
    });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });

    expect(result.current.error).toBe(error);
  });
});
