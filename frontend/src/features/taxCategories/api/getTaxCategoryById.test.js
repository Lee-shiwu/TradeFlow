import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiRequest } from "../../../shared/api/apiClient";
import { getTaxCategoryById } from "./getTaxCategoryById";

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: vi.fn(),
}));

describe("getTaxCategoryById", () => {
  beforeEach(() => {
    vi.mocked(apiRequest).mockReset();
  });

  it("requests the selected tax category", async () => {
    const response = { taxCategoryId: "tax-category-id" };
    vi.mocked(apiRequest).mockResolvedValue(response);

    await expect(getTaxCategoryById("tax-category-id")).resolves.toBe(response);
    expect(apiRequest).toHaveBeenCalledWith(
      "/api/v1/catalog/tax-categories/tax-category-id",
      { method: "GET" },
    );
  });

  it("encodes the tax category ID", async () => {
    vi.mocked(apiRequest).mockResolvedValue({});

    await getTaxCategoryById("tax/category");

    expect(apiRequest).toHaveBeenCalledWith(
      "/api/v1/catalog/tax-categories/tax%2Fcategory",
      { method: "GET" },
    );
  });
});
