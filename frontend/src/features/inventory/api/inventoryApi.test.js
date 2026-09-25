import { afterEach, describe, expect, it, vi } from "vitest";
import { receivePurchaseOrder } from "../../purchaseOrders/api/receivePurchaseOrder";
import { listStock } from "./listStock";

afterEach(() => vi.unstubAllGlobals());

describe("goods receipt and inventory API", () => {
  it("receives a confirmed purchase order with its row version", async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ goodsReceiptId: "receipt-1" }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      }),
    );
    vi.stubGlobal("fetch", fetchMock);

    await receivePurchaseOrder("order/1", "AQID");

    expect(fetchMock).toHaveBeenCalledWith(
      "/api/v1/purchasing/purchase-orders/order%2F1/receive",
      expect.objectContaining({
        method: "POST",
        body: JSON.stringify({ rowVersion: "AQID" }),
      }),
    );
  });

  it("lists stock using search and paging", async () => {
    const fetchMock = vi.fn().mockResolvedValue(
      new Response(JSON.stringify({ items: [], totalCount: 0 }), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      }),
    );
    vi.stubGlobal("fetch", fetchMock);

    await listStock({ search: "SKU-1", pageNumber: 2, pageSize: 20 });

    expect(fetchMock.mock.calls[0][0]).toBe(
      "/api/v1/inventory/stock?pageNumber=2&pageSize=20&search=SKU-1",
    );
  });
});
