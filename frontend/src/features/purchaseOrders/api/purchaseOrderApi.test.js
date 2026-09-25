import { afterEach, describe, expect, it, vi } from "vitest";
import { createPurchaseOrder } from "./createPurchaseOrder";
import { listPurchaseOrders } from "./listPurchaseOrders";

afterEach(() => vi.unstubAllGlobals());

describe("purchase order API", () => {
  it("creates a purchase order with its lines", async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ purchaseOrderId: "order-1" }), {
        status: 201,
        headers: { "Content-Type": "application/json" },
      }),
    );
    vi.stubGlobal("fetch", fetchMock);

    await createPurchaseOrder({
      supplierId: "supplier-1",
      reference: "PO-1",
      lines: [{ productId: "product-1", quantity: 2, unitPrice: 5 }],
    });

    expect(fetchMock).toHaveBeenCalledWith(
      "/api/v1/purchasing/purchase-orders",
      expect.objectContaining({ method: "POST" }),
    );
  });

  it("lists purchase orders with paging and search", async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ items: [], totalCount: 0 }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      }),
    );
    vi.stubGlobal("fetch", fetchMock);

    await listPurchaseOrders({ search: "PO-1", pageNumber: 2, pageSize: 20 });

    expect(fetchMock.mock.calls[0][0]).toBe(
      "/api/v1/purchasing/purchase-orders?pageNumber=2&pageSize=20&search=PO-1",
    );
  });
});
