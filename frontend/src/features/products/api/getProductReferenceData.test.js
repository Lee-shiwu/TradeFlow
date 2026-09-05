import { beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { getProductReferenceData } from "./getProductReferenceData";

const apiRequestMock = vi.hoisted(() => vi.fn());

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: apiRequestMock,
}));

const referenceData = {
  unitsOfMeasure: [
    {
      id: "10000000-0000-0000-0000-000000000001",
      code: "EA",
      name: "Each",
    },
  ],
  productCategories: [
    {
      id: "30000000-0000-0000-0000-000000000001",
      code: "OFFICE",
      name: "Office products",
    },
  ],
  taxCategories: [
    {
      id: "20000000-0000-0000-0000-000000000001",
      code: "GST15",
      name: "Standard GST",
    },
  ],
};

describe("getProductReferenceData", () => {
  beforeEach(() => {
    apiRequestMock.mockReset();
  });

  it("requests product reference data using GET", async () => {
    apiRequestMock.mockResolvedValue(referenceData);

    await getProductReferenceData();

    expect(apiRequestMock).toHaveBeenCalledOnce();
    expect(apiRequestMock).toHaveBeenCalledWith(
      "/api/v1/catalog/products/reference-data",
      {
        method: "GET",
      },
    );
  });

  it("returns all product reference data groups", async () => {
    apiRequestMock.mockResolvedValue(referenceData);

    const result = await getProductReferenceData();

    expect(result).toEqual(referenceData);
  });

  it("propagates an API error", async () => {
    const error = new ApiError(
      500,
      "UNEXPECTED_ERROR",
      "Unable to load product reference data.",
      "reference-data-trace-id",
    );

    apiRequestMock.mockRejectedValue(error);

    await expect(getProductReferenceData()).rejects.toBe(error);
  });
});
