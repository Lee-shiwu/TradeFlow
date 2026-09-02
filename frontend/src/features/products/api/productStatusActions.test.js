import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { activateProduct } from "./activateProduct";
import { deactivateProduct } from "./deactivateProduct";

const apiRequestMock = vi.hoisted(() => vi.fn());

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));

const productId = "33333333-3333-3333-3333-333333333333";
const rowVersion = "AAAAAAAAB9E=";

const activatedProduct = {
  productId,
  status: "Active",
  lastModifiedAt: "2026-09-01T00:30:00+00:00",
  lastModifiedBy: "66666666-6666-6666-6666-666666666666",
  rowVersion: "AAAAAAAAB9I=",
};

const deactivatedProduct = {
  productId,
  status: "Inactive",
  lastModifiedAt: "2026-09-01T00:45:00+00:00",
  lastModifiedBy: "66666666-6666-6666-6666-666666666666",
  rowVersion: "AAAAAAAAB9M=",
};

describe("product status API actions", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
  });

  it("requests the selected product activation", async () => {
    apiRequestMock.mockResolvedValue(activatedProduct);

    await activateProduct(productId, rowVersion);

    expect(apiRequestMock).toHaveBeenCalledOnce();

    expect(apiRequestMock).toHaveBeenCalledWith(
      `/api/v1/catalog/products/${productId}/activate`,
      {
        method: "POST",
        body: JSON.stringify({
          rowVersion,
        }),
      },
    );
  });

  it("returns the activated product information", async () => {
    apiRequestMock.mockResolvedValue(activatedProduct);

    const result = await activateProduct(productId, rowVersion);

    expect(result).toEqual(activatedProduct);
    expect(result.status).toBe("Active");
    expect(result.rowVersion).toBe("AAAAAAAAB9I=");
  });

  it("requests the selected product deactivation", async () => {
    apiRequestMock.mockResolvedValue(deactivatedProduct);

    await deactivateProduct(productId, rowVersion);

    expect(apiRequestMock).toHaveBeenCalledOnce();

    expect(apiRequestMock).toHaveBeenCalledWith(
      `/api/v1/catalog/products/${productId}/deactivate`,
      {
        method: "POST",
        body: JSON.stringify({
          rowVersion,
        }),
      },
    );
  });

  it("returns the deactivated product information", async () => {
    apiRequestMock.mockResolvedValue(deactivatedProduct);

    const result = await deactivateProduct(productId, rowVersion);

    expect(result).toEqual(deactivatedProduct);
    expect(result.status).toBe("Inactive");
    expect(result.rowVersion).toBe("AAAAAAAAB9M=");
  });

  it("propagates a concurrency conflict", async () => {
    const error = new ApiError(
      409,
      "PRODUCT_CONCURRENCY_CONFLICT",
      "The product was modified by another request.",
      "concurrency-trace-id",
    );

    apiRequestMock.mockRejectedValue(error);

    await expect(deactivateProduct(productId, rowVersion)).rejects.toBe(error);
  });

  it("propagates a product-not-found error", async () => {
    const error = new ApiError(
      404,
      "PRODUCT_NOT_FOUND",
      "The selected product does not exist.",
      "not-found-trace-id",
    );

    apiRequestMock.mockRejectedValue(error);

    await expect(activateProduct(productId, rowVersion)).rejects.toBe(error);
  });
});
