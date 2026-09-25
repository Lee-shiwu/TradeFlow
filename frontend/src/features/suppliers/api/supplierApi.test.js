import { beforeEach, describe, expect, it, vi } from "vitest";
import { apiRequest } from "../../../shared/api/apiClient";
import { createSupplier } from "./createSupplier";
import { listSuppliers } from "./listSuppliers";

vi.mock("../../../shared/api/apiClient", () => ({
  apiRequest: vi.fn(),
}));

describe("supplier API", () => {
  beforeEach(() => {
    vi.mocked(apiRequest).mockReset();
  });

  it("builds list search parameters", async () => {
    vi.mocked(apiRequest).mockResolvedValue({ items: [] });

    await listSuppliers({
      search: " office ",
      status: "Active",
      pageNumber: 2,
      pageSize: 20,
    });

    expect(apiRequest).toHaveBeenCalledWith(
      "/api/v1/purchasing/suppliers?pageNumber=2&pageSize=20" +
        "&search=+office+&status=Active",
      { method: "GET" },
    );
  });

  it("posts a supplier request", async () => {
    const request = { code: "OFFICE", name: "Office supplier" };
    vi.mocked(apiRequest).mockResolvedValue({ supplierId: "supplier-id" });

    await createSupplier(request);

    expect(apiRequest).toHaveBeenCalledWith("/api/v1/purchasing/suppliers", {
      method: "POST",
      body: JSON.stringify(request),
    });
  });
});
