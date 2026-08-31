import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "./ApiError";
import { apiRequest } from "./apiClient";
const organisationId = "11111111-1111-1111-1111-111111111111";
const userId = "22222222-2222-2222-2222-222222222222";
function stubFetch(response) {
  const fetchMock = vi.fn().mockResolvedValue(response);
  vi.stubGlobal("fetch", fetchMock);
  return fetchMock;
}
describe("apiRequest", () => {
  beforeEach(() => {
    vi.stubEnv("VITE_TEMP_ORGANISATION_ID", organisationId);
    vi.stubEnv("VITE_TEMP_USER_ID", userId);
  });
  afterEach(() => {
    vi.unstubAllEnvs();
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });
  it("adds identity and accept headers and returns JSON", async () => {
    const responseBody = {
      productId: "33333333-3333-3333-3333-333333333333",
      sku: "CHAIR-001",
    };
    const fetchMock = stubFetch(
      new Response(JSON.stringify(responseBody), {
        status: 200,
        headers: {
          "Content-Type": "application/json",
        },
      }),
    );
    const result = await apiRequest(
      "/api/v1/catalog/products/33333333-3333-3333-3333-333333333333",
    );
    expect(result).toEqual(responseBody);
    expect(fetchMock).toHaveBeenCalledOnce();
    const [path, request] = fetchMock.mock.calls[0];
    const headers = request.headers;
    expect(path).toBe(
      "/api/v1/catalog/products/33333333-3333-3333-3333-333333333333",
    );
    expect(headers.get("Accept")).toBe("application/json");
    expect(headers.get("X-Organisation-Id")).toBe(organisationId);
    expect(headers.get("X-User-Id")).toBe(userId);
  });
  it("adds JSON content type when request has a body", async () => {
    const fetchMock = stubFetch(
      new Response(null, {
        status: 204,
      }),
    );
    await apiRequest("/api/v1/catalog/products", {
      method: "POST",
      body: JSON.stringify({
        sku: "CHAIR-001",
      }),
    });
    const [, request] = fetchMock.mock.calls[0];
    const headers = request.headers;
    expect(headers.get("Content-Type")).toBe("application/json");
  });
  it("does not replace an existing content type", async () => {
    const fetchMock = stubFetch(
      new Response(null, {
        status: 204,
      }),
    );
    await apiRequest("/api/v1/catalog/import", {
      method: "POST",
      headers: {
        "Content-Type": "text/plain",
      },
      body: "CHAIR-001",
    });
    const [, request] = fetchMock.mock.calls[0];
    const headers = request.headers;
    expect(headers.get("Content-Type")).toBe("text/plain");
  });
  it("returns undefined for a no-content response", async () => {
    stubFetch(
      new Response(null, {
        status: 204,
      }),
    );
    const result = await apiRequest(
      "/api/v1/catalog/products/33333333-3333-3333-3333-333333333333/deactivate",
      {
        method: "POST",
      },
    );
    expect(result).toBeUndefined();
  });
  it("converts an API problem response into ApiError", async () => {
    stubFetch(
      new Response(
        JSON.stringify({
          title: "Business rule conflict",
          status: 409,
          code: "PRODUCT_SKU_ALREADY_EXISTS",
          detail: "A product with the same SKU already exists.",
          traceId: "test-trace-id",
          errors: {
            Sku: ["The SKU is already in use."],
          },
        }),
        {
          status: 409,
          headers: {
            "Content-Type": "application/problem+json",
          },
        },
      ),
    );
    const request = apiRequest("/api/v1/catalog/products", {
      method: "POST",
      body: JSON.stringify({
        sku: "CHAIR-001",
      }),
    });
    await expect(request).rejects.toBeInstanceOf(ApiError);
    await expect(request).rejects.toMatchObject({
      status: 409,
      code: "PRODUCT_SKU_ALREADY_EXISTS",
      detail: "A product with the same SKU already exists.",
      traceId: "test-trace-id",
      errors: {
        Sku: ["The SKU is already in use."],
      },
    });
  });
  it("uses the problem title when detail is missing", async () => {
    stubFetch(
      new Response(
        JSON.stringify({
          title: "Resource not found",
          status: 404,
          code: "PRODUCT_NOT_FOUND",
          traceId: "not-found-trace-id",
        }),
        {
          status: 404,
          headers: {
            "Content-Type": "application/problem+json",
          },
        },
      ),
    );
    const request = apiRequest(
      "/api/v1/catalog/products/33333333-3333-3333-3333-333333333333",
    );
    await expect(request).rejects.toMatchObject({
      status: 404,
      code: "PRODUCT_NOT_FOUND",
      detail: "Resource not found",
      traceId: "not-found-trace-id",
    });
  });
  it("creates a fallback ApiError for a non-JSON error response", async () => {
    stubFetch(
      new Response("Bad Gateway", {
        status: 502,
        headers: {
          "Content-Type": "text/plain",
        },
      }),
    );
    const request = apiRequest("/api/v1/catalog/products");
    await expect(request).rejects.toBeInstanceOf(ApiError);
    await expect(request).rejects.toMatchObject({
      status: 502,
      code: "HTTP_ERROR",
      detail: "Request failed with status 502.",
    });
  });
});
