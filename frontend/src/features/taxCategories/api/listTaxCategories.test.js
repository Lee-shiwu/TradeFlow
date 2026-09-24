import { beforeEach, describe, expect, it, vi } from "vitest";
import { listTaxCategories } from "./listTaxCategories";

const apiRequestMock = vi.hoisted(() => vi.fn());

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));

const response = {
  items: [],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 0,
};

describe("listTaxCategories", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
    apiRequestMock.mockResolvedValue(response);
  });

  it("sends search, status, and pagination parameters", async () => {
    await expect(
      listTaxCategories({
        search: "gst",
        status: "Active",
        pageNumber: 2,
        pageSize: 20,
      }),
    ).resolves.toBe(response);

    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/tax-categories" +
        "?search=gst" +
        "&status=Active" +
        "&pageNumber=2" +
        "&pageSize=20",
      { method: "GET" },
    );
  });

  it("trims and encodes search text", async () => {
    await listTaxCategories({
      search: "  GST & exempt  ",
      pageNumber: 1,
      pageSize: 10,
    });

    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/tax-categories" +
        "?search=GST+%26+exempt" +
        "&pageNumber=1" +
        "&pageSize=10",
      { method: "GET" },
    );
  });

  it("omits blank optional filters", async () => {
    await listTaxCategories({
      search: "   ",
      pageNumber: 1,
      pageSize: 50,
    });

    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/tax-categories?pageNumber=1&pageSize=50",
      { method: "GET" },
    );
  });
});
