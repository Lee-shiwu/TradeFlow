import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { updateProductDetails } from "./updateProductDetails";

const apiRequestMock = vi.hoisted(() => vi.fn());

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));

const productId = "33333333-3333-3333-3333-333333333333";

const request = {
  name: "Updated Office Chair",
  description: "Updated ergonomic office chair.",
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
  rowVersion: "AAAAAAAAB9E=",
};

const response = {
  productId,
  name: "Updated Office Chair",
  description: "Updated ergonomic office chair.",
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
  lastModifiedAt: "2026-09-03T01:30:00+00:00",
  lastModifiedBy: "88888888-8888-8888-8888-888888888888",
  rowVersion: "AAAAAAAAB9F=",
};

describe("updateProductDetails", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
  });

  it("sends the update request to the selected product", async () => {
    apiRequestMock.mockResolvedValue(response);

    await updateProductDetails(productId, request);

    expect(apiRequestMock).toHaveBeenCalledOnce();

    expect(apiRequestMock).toHaveBeenCalledWith(
      `/api/v1/catalog/products/${productId}`,
      {
        method: "PUT",
        body: JSON.stringify(request),
      },
    );
  });

  it("returns the updated product details", async () => {
    apiRequestMock.mockResolvedValue(response);

    const result = await updateProductDetails(productId, request);

    expect(result).toEqual(response);

    expect(result.rowVersion).toBe("AAAAAAAAB9F=");
  });

  it("allows the product category to be cleared", async () => {
    apiRequestMock.mockResolvedValue({
      ...response,
      productCategoryId: null,
    });

    const requestWithoutProductCategory = {
      ...request,
      productCategoryId: null,
    };

    await updateProductDetails(productId, requestWithoutProductCategory);

    expect(apiRequestMock).toHaveBeenCalledWith(
      `/api/v1/catalog/products/${productId}`,
      {
        method: "PUT",
        body: JSON.stringify(requestWithoutProductCategory),
      },
    );
  });

  it("propagates a validation error", async () => {
    const apiError = new ApiError(
      400,
      "UPDATE_PRODUCT_NAME_REQUIRED",
      "Product name is required.",
      "validation-trace-id",
      {
        Name: ["Product name is required."],
      },
    );

    apiRequestMock.mockRejectedValue(apiError);

    await expect(updateProductDetails(productId, request)).rejects.toBe(
      apiError,
    );
  });

  it("propagates a product not-found error", async () => {
    const apiError = new ApiError(
      404,
      "PRODUCT_NOT_FOUND",
      "The selected product does not exist.",
      "not-found-trace-id",
    );

    apiRequestMock.mockRejectedValue(apiError);

    await expect(updateProductDetails(productId, request)).rejects.toBe(
      apiError,
    );
  });

  it("propagates a concurrency conflict", async () => {
    const apiError = new ApiError(
      409,
      "PRODUCT_CONCURRENCY_CONFLICT",
      "The product was changed by another request.",
      "concurrency-trace-id",
    );

    apiRequestMock.mockRejectedValue(apiError);

    await expect(updateProductDetails(productId, request)).rejects.toBe(
      apiError,
    );
  });
});
