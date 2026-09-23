import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { getProductCategoryById } from "./getProductCategoryById";

const apiRequestMock = vi.hoisted(() => vi.fn());

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));

const productCategoryId = "77777777-7777-7777-7777-777777777777";

const details = {
  productCategoryId,
  code: "OFFICE",
  name: "Office products",
  description: "Products used in the office.",
  status: "Active",
  createdAt: "2026-09-20T10:00:00+00:00",
  createdBy: "11111111-1111-1111-1111-111111111111",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

describe("getProductCategoryById", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
  });

  it("requests the selected category using GET", async () => {
    apiRequestMock.mockResolvedValue(details);

    await getProductCategoryById(productCategoryId);

    expect(apiRequestMock).toHaveBeenCalledWith(
      `/api/v1/catalog/product-categories/${productCategoryId}`,
      { method: "GET" },
    );
  });

  it("encodes the category ID before building the path", async () => {
    apiRequestMock.mockResolvedValue(details);

    await getProductCategoryById("category/id");

    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/product-categories/category%2Fid",
      { method: "GET" },
    );
  });

  it("propagates API errors", async () => {
    const error = new ApiError(
      404,
      "PRODUCT_CATEGORY_NOT_FOUND",
      "The requested product category was not found.",
    );
    apiRequestMock.mockRejectedValue(error);

    await expect(getProductCategoryById(productCategoryId)).rejects.toBe(error);
  });
});
