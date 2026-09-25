import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiRequest } from "../../../shared/api/apiClient";
import { createTaxCategory } from "./createTaxCategory";

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: vi.fn(),
}));

describe("createTaxCategory", () => {
  beforeEach(() => {
    vi.mocked(apiRequest).mockReset();
  });

  it("posts the tax category request", async () => {
    const request = {
      code: "REGIONAL",
      name: "Regional levy",
      description: null,
      rate: 0.125,
      treatment: "StandardRated",
      effectiveFrom: "2026-10-01",
      effectiveTo: null,
    };
    const response = { taxCategoryId: "tax-category-id", ...request };
    vi.mocked(apiRequest).mockResolvedValue(response);

    await expect(createTaxCategory(request)).resolves.toBe(response);
    expect(apiRequest).toHaveBeenCalledWith("/api/v1/catalog/tax-categories", {
      method: "POST",
      body: JSON.stringify(request),
    });
  });
});
