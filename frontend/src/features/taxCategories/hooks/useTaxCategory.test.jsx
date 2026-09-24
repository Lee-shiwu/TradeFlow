import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { getTaxCategoryById } from "../api/getTaxCategoryById";
import { useTaxCategory } from "./useTaxCategory";

vi.mock("../api/getTaxCategoryById", () => ({
  getTaxCategoryById: vi.fn(),
}));

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

describe("useTaxCategory", () => {
  beforeEach(() => {
    vi.mocked(getTaxCategoryById).mockReset();
  });

  it("loads details for the selected ID", async () => {
    const details = { taxCategoryId: "tax-category-id", code: "GST15" };
    vi.mocked(getTaxCategoryById).mockResolvedValue(details);

    const { result } = renderHook(() => useTaxCategory("tax-category-id"), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isSuccess).toBe(true));
    expect(result.current.data).toBe(details);
    expect(getTaxCategoryById).toHaveBeenCalledWith("tax-category-id");
  });

  it.each([undefined, ""])(
    "does not request details when the ID is %s",
    async (missingTaxCategoryId) => {
      const { result } = renderHook(
        () => useTaxCategory(missingTaxCategoryId),
        { wrapper: createWrapper() },
      );

      expect(result.current.fetchStatus).toBe("idle");
      expect(getTaxCategoryById).not.toHaveBeenCalled();
    },
  );
});
