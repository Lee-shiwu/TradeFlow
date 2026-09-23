import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { createProductCategory } from "./createProductCategory";

const apiRequestMock = vi.hoisted(() => vi.fn());

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));

const request = {
  code: "OFFICE",
  name: "Office products",
  description: "Products used in the office.",
};

const response = {
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  ...request,
  status: "Active",
  createdAt: "2026-09-23T08:30:00+00:00",
  createdBy: "11111111-1111-1111-1111-111111111111",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

describe("createProductCategory", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
  });

  it("posts the category request as JSON", async () => {
    apiRequestMock.mockResolvedValue(response);

    await createProductCategory(request);

    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/product-categories",
      {
        method: "POST",
        body: JSON.stringify(request),
      },
    );
  });

  it("returns the created category", async () => {
    apiRequestMock.mockResolvedValue(response);

    await expect(createProductCategory(request)).resolves.toEqual(response);
  });

  it("propagates API errors", async () => {
    const error = new ApiError(
      409,
      "PRODUCT_CATEGORY_CODE_ALREADY_EXISTS",
      "A product category with this code already exists in the organisation.",
    );
    apiRequestMock.mockRejectedValue(error);

    await expect(createProductCategory(request)).rejects.toBe(error);
  });
});
