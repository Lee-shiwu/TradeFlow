import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { updateProductCategoryDetails } from "./updateProductCategoryDetails";

const apiRequestMock = vi.hoisted(() => vi.fn());

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));

const productCategoryId = "77777777-7777-7777-7777-777777777777";
const request = {
  name: "Updated category",
  description: "Updated description.",
  rowVersion: "AAAAAAAAB9E=",
};

describe("updateProductCategoryDetails", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
  });

  it("puts the edited details and row version as JSON", async () => {
    apiRequestMock.mockResolvedValue({ productCategoryId, ...request });

    await updateProductCategoryDetails(productCategoryId, request);

    expect(apiRequestMock).toHaveBeenCalledWith(
      `/api/v1/catalog/product-categories/${productCategoryId}`,
      {
        method: "PUT",
        body: JSON.stringify(request),
      },
    );
  });

  it("propagates concurrency errors", async () => {
    const error = new ApiError(
      409,
      "UPDATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT",
      "The category changed.",
    );
    apiRequestMock.mockRejectedValue(error);

    await expect(
      updateProductCategoryDetails(productCategoryId, request),
    ).rejects.toBe(error);
  });
});
