/** @typedef {import('./productListTypes.js').ListProductsResponse} ListProductsResponse */

import { beforeEach, describe, expect, it, vi } from "vitest";
import { listProducts } from "./listProducts";
const apiRequestMock = vi.hoisted(() => vi.fn());
vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));
const emptyResponse = {
  items: [],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 0,
};
describe("listProducts", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
    apiRequestMock.mockResolvedValue(emptyResponse);
  });
  it("sends search, status and pagination parameters", async () => {
    const response = await listProducts({
      search: "chair",
      status: "Active",
      pageNumber: 2,
      pageSize: 20,
    });
    expect(apiRequestMock).toHaveBeenCalledOnce();
    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/products" +
        "?search=chair" +
        "&status=Active" +
        "&pageNumber=2" +
        "&pageSize=20",
      {
        method: "GET",
      },
    );
    expect(response).toBe(emptyResponse);
  });
  it("trims search text before adding it to the URL", async () => {
    await listProducts({
      search: "   chair desk   ",
      pageNumber: 1,
      pageSize: 20,
    });
    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/products" +
        "?search=chair+desk" +
        "&pageNumber=1" +
        "&pageSize=20",
      {
        method: "GET",
      },
    );
  });
  it("omits blank search and missing status", async () => {
    await listProducts({
      search: "   ",
      pageNumber: 1,
      pageSize: 50,
    });
    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/products" + "?pageNumber=1" + "&pageSize=50",
      {
        method: "GET",
      },
    );
  });
  it("encodes special characters in search text", async () => {
    await listProducts({
      search: "desk & chair/large",
      status: "Inactive",
      pageNumber: 3,
      pageSize: 10,
    });
    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/products" +
        "?search=desk+%26+chair%2Flarge" +
        "&status=Inactive" +
        "&pageNumber=3" +
        "&pageSize=10",
      {
        method: "GET",
      },
    );
  });
  it("returns the response received from apiRequest", async () => {
    const expectedResponse = {
      items: [
        {
          productId: "33333333-3333-3333-3333-333333333333",
          sku: "CHAIR-001",
          name: "Office Chair",
          unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
          productCategoryId: null,
          taxCategoryId: "55555555-5555-5555-5555-555555555555",
          status: "Active",
          createdAt: "2026-08-27T00:00:00+00:00",
          lastModifiedAt: null,
        },
      ],
      pageNumber: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
    };
    apiRequestMock.mockResolvedValue(expectedResponse);
    const response = await listProducts({
      pageNumber: 1,
      pageSize: 20,
    });
    expect(response).toBe(expectedResponse);
  });
});
