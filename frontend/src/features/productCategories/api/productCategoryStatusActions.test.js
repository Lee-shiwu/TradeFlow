import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { activateProductCategory } from "./activateProductCategory";
import { deactivateProductCategory } from "./deactivateProductCategory";

const apiRequestMock = vi.hoisted(() => vi.fn());

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));

const productCategoryId = "77777777-7777-7777-7777-777777777777";
const rowVersion = "AAAAAAAAB9E=";
const activatedResponse = {
  productCategoryId,
  status: "Active",
  lastModifiedAt: "2026-09-24T01:00:00+00:00",
  lastModifiedBy: "11111111-1111-1111-1111-111111111111",
  rowVersion: "AAAAAAAAB9I=",
};
const deactivatedResponse = {
  ...activatedResponse,
  status: "Inactive",
};

describe("product category status API actions", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
  });

  it("posts an activation request", async () => {
    apiRequestMock.mockResolvedValue(activatedResponse);

    await expect(
      activateProductCategory(productCategoryId, rowVersion),
    ).resolves.toEqual(activatedResponse);
    expect(apiRequestMock).toHaveBeenCalledWith(
      `/api/v1/catalog/product-categories/${productCategoryId}/activate`,
      {
        method: "POST",
        body: JSON.stringify({ rowVersion }),
      },
    );
  });

  it("posts a deactivation request", async () => {
    apiRequestMock.mockResolvedValue(deactivatedResponse);

    await expect(
      deactivateProductCategory(productCategoryId, rowVersion),
    ).resolves.toEqual(deactivatedResponse);
    expect(apiRequestMock).toHaveBeenCalledWith(
      `/api/v1/catalog/product-categories/${productCategoryId}/deactivate`,
      {
        method: "POST",
        body: JSON.stringify({ rowVersion }),
      },
    );
  });

  it("propagates API errors", async () => {
    const error = new ApiError(
      409,
      "DEACTIVATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT",
      "Category changed.",
    );
    apiRequestMock.mockRejectedValue(error);

    let caughtError;

    try {
      await deactivateProductCategory(productCategoryId, rowVersion);
    } catch (requestError) {
      caughtError = requestError;
    }

    expect(caughtError).toBe(error);
  });
});
