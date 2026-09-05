import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { createProduct } from "./createProduct";

const apiRequestMock = vi.hoisted(() => vi.fn());

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));

const request = {
  sku: "CHAIR-001",
  name: "Office Chair",
  description: "Ergonomic office chair.",
  unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
};

const response = {
  productId: "33333333-3333-3333-3333-333333333333",
  sku: "CHAIR-001",
};

describe("createProduct", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
  });

  it("sends the product creation request", async () => {
    apiRequestMock.mockResolvedValue(response);

    await createProduct(request);

    expect(apiRequestMock).toHaveBeenCalledOnce();

    expect(apiRequestMock).toHaveBeenCalledWith("/api/v1/catalog/products", {
      method: "POST",
      body: JSON.stringify(request),
    });
  });

  it("returns the created product information", async () => {
    apiRequestMock.mockResolvedValue(response);

    const result = await createProduct(request);

    expect(result).toEqual(response);
    expect(result.productId).toBe("33333333-3333-3333-3333-333333333333");
    expect(result.sku).toBe("CHAIR-001");
  });

  it("allows the optional product category to be null", async () => {
    apiRequestMock.mockResolvedValue(response);

    const requestWithoutProductCategory = {
      ...request,
      productCategoryId: null,
    };

    await createProduct(requestWithoutProductCategory);

    expect(apiRequestMock).toHaveBeenCalledWith("/api/v1/catalog/products", {
      method: "POST",
      body: JSON.stringify(requestWithoutProductCategory),
    });
  });

  it("allows the optional description to be null", async () => {
    apiRequestMock.mockResolvedValue(response);

    const requestWithoutDescription = {
      ...request,
      description: null,
    };

    await createProduct(requestWithoutDescription);

    expect(apiRequestMock).toHaveBeenCalledWith("/api/v1/catalog/products", {
      method: "POST",
      body: JSON.stringify(requestWithoutDescription),
    });
  });

  it("propagates a validation error", async () => {
    const apiError = new ApiError(
      400,
      "CREATE_PRODUCT_NAME_REQUIRED",
      "Product name is required.",
      "validation-trace-id",
      {
        Name: ["Product name is required."],
      },
    );

    apiRequestMock.mockRejectedValue(apiError);

    await expect(createProduct(request)).rejects.toBe(apiError);
  });

  it("propagates a duplicate SKU conflict", async () => {
    const apiError = new ApiError(
      409,
      "CREATE_PRODUCT_SKU_ALREADY_EXISTS",
      "A product with the same SKU already exists.",
      "conflict-trace-id",
    );

    apiRequestMock.mockRejectedValue(apiError);

    await expect(createProduct(request)).rejects.toBe(apiError);
  });
});
