import { beforeEach, describe, expect, it, vi } from "vitest";
import { listProductCategories } from "./listProductCategories";

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

describe("listProductCategories", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
    apiRequestMock.mockResolvedValue(response);
  });

  it("sends search, status, and pagination parameters", async () => {
    const result = await listProductCategories({
      search: "office",
      status: "Active",
      pageNumber: 2,
      pageSize: 20,
    });

    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/product-categories" +
        "?search=office" +
        "&status=Active" +
        "&pageNumber=2" +
        "&pageSize=20",
      { method: "GET" },
    );
    expect(result).toBe(response);
  });

  it("trims and encodes search text", async () => {
    await listProductCategories({
      search: "  office & furniture  ",
      pageNumber: 1,
      pageSize: 10,
    });

    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/product-categories" +
        "?search=office+%26+furniture" +
        "&pageNumber=1" +
        "&pageSize=10",
      { method: "GET" },
    );
  });

  it("omits blank search and missing status", async () => {
    await listProductCategories({
      search: "   ",
      pageNumber: 1,
      pageSize: 50,
    });

    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/product-categories?pageNumber=1&pageSize=50",
      { method: "GET" },
    );
  });
});
