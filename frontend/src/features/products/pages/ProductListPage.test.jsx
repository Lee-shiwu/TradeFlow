/**
 * @typedef {import("../api/productListTypes.js").ListProductsResponse}
 * ListProductsResponse
 */

import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes, useParams } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { ProductListPage } from "./ProductListPage";

const useProductsMock = vi.hoisted(() => vi.fn());

const useCreateProductMock = vi.hoisted(() => vi.fn());

const createProductDialogMock = vi.hoisted(() => vi.fn());

const refetchMock = vi.hoisted(() => vi.fn());

const createMutateAsyncMock = vi.hoisted(() => vi.fn());

const createResetMock = vi.hoisted(() => vi.fn());

vi.mock("../hooks/useProducts", () => ({
  useProducts: useProductsMock,
}));

vi.mock("../hooks/useCreateProduct", () => ({
  useCreateProduct: useCreateProductMock,
}));

vi.mock("../components/CreateProductDialog", () => ({
  CreateProductDialog: (properties) => {
    createProductDialogMock(properties);

    if (!properties.open) {
      return null;
    }

    return (
      <div role="dialog" aria-label="Create product dialog">
        <span>
          {properties.isPending ? "Creating product" : "Ready to create"}
        </span>

        {properties.error && (
          <span>{properties.error.detail ?? "Unknown creation error"}</span>
        )}

        <button
          type="button"
          onClick={() => {
            void properties.onSubmit(createProductRequest);
          }}
        >
          Submit mocked product
        </button>

        <button type="button" onClick={properties.onClose}>
          Close mocked product
        </button>
      </div>
    );
  },
}));

const productId = "33333333-3333-3333-3333-333333333333";

const createProductRequest = {
  sku: "CHAIR-002",
  name: "New Office Chair",
  description: "New ergonomic chair.",
  unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
};

const createProductResponse = {
  productId: "99999999-9999-9999-9999-999999999999",
  sku: "CHAIR-002",
};

const productListResponse = {
  items: [
    {
      productId,
      sku: "CHAIR-001",
      name: "Office Chair",
      unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
      productCategoryId: null,
      taxCategoryId: "55555555-5555-5555-5555-555555555555",
      status: "Active",
      createdAt: "2026-08-27T00:00:00+00:00",
      lastModifiedAt: null,
    },
  ],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 1,
  totalPages: 1,
};

const emptyResponse = {
  items: [],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 0,
  totalPages: 0,
};

function configureQuery(values = {}) {
  useProductsMock.mockReturnValue({
    data: values.data,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isFetching: values.isFetching ?? false,
    refetch: refetchMock,
  });
}

function configureCreateMutation(values = {}) {
  useCreateProductMock.mockReturnValue({
    data: values.data ?? createProductResponse,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isSuccess: values.isSuccess ?? false,
    mutateAsync: createMutateAsyncMock,
    reset: createResetMock,
  });
}

function ProductDetailsRouteStub() {
  const { productId: routeProductId } = useParams();

  return <h1>Product details: {routeProductId}</h1>;
}

function renderPage() {
  return render(
    <MemoryRouter initialEntries={["/products"]}>
      <Routes>
        <Route path="/products" element={<ProductListPage />} />

        <Route
          path="/products/:productId"
          element={<ProductDetailsRouteStub />}
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe("ProductListPage", () => {
  beforeEach(() => {
    useProductsMock.mockReset();
    useCreateProductMock.mockReset();
    createProductDialogMock.mockReset();
    refetchMock.mockReset();
    createMutateAsyncMock.mockReset();
    createResetMock.mockReset();

    createMutateAsyncMock.mockResolvedValue(createProductResponse);

    configureCreateMutation();
  });

  afterEach(() => {
    cleanup();
  });

  it("shows a loading message while products are loading", () => {
    configureQuery({
      isPending: true,
      isFetching: true,
    });

    renderPage();

    expect(screen.getByText("Loading products...")).toBeInTheDocument();
  });

  it("shows products in the product table", () => {
    configureQuery({
      data: productListResponse,
    });

    renderPage();

    expect(screen.getByText("Total products: 1")).toBeInTheDocument();

    expect(
      screen.getByRole("link", {
        name: "CHAIR-001",
      }),
    ).toBeInTheDocument();

    expect(screen.getByText("Office Chair")).toBeInTheDocument();

    expect(screen.getByText("Active")).toBeInTheDocument();
  });

  it("shows an empty message when no products exist", () => {
    configureQuery({
      data: emptyResponse,
    });

    renderPage();

    expect(screen.getByText("No products found.")).toBeInTheDocument();

    expect(screen.queryByRole("table")).not.toBeInTheDocument();
  });

  it("shows the API error and retries the request", async () => {
    const user = userEvent.setup();

    configureQuery({
      error: new ApiError(
        500,
        "UNEXPECTED_ERROR",
        "The product service is unavailable.",
        "test-trace-id",
      ),
      isError: true,
    });

    renderPage();

    expect(
      screen.getByText("The product service is unavailable."),
    ).toBeInTheDocument();

    await user.click(
      screen.getByRole("button", {
        name: "Retry",
      }),
    );

    expect(refetchMock).toHaveBeenCalledOnce();
  });

  it("opens the selected product details page", async () => {
    const user = userEvent.setup();

    configureQuery({
      data: productListResponse,
    });

    renderPage();

    const productLink = screen.getByRole("link", {
      name: "CHAIR-001",
    });

    expect(productLink).toHaveAttribute("href", `/products/${productId}`);

    await user.click(productLink);

    expect(
      await screen.findByRole("heading", {
        name: `Product details: ${productId}`,
      }),
    ).toBeInTheDocument();
  });

  it("shows the create product button", () => {
    configureQuery({
      data: productListResponse,
    });

    renderPage();

    expect(
      screen.getByRole("button", {
        name: "Create product",
      }),
    ).toBeInTheDocument();
  });

  it("opens the create product dialog", async () => {
    const user = userEvent.setup();

    configureQuery({
      data: productListResponse,
    });

    renderPage();

    expect(
      screen.queryByRole("dialog", {
        name: "Create product dialog",
      }),
    ).not.toBeInTheDocument();

    await user.click(
      screen.getByRole("button", {
        name: "Create product",
      }),
    );

    expect(
      screen.getByRole("dialog", {
        name: "Create product dialog",
      }),
    ).toBeInTheDocument();

    expect(createResetMock).toHaveBeenCalledOnce();

    expect(createProductDialogMock).toHaveBeenLastCalledWith(
      expect.objectContaining({
        open: true,
      }),
    );
  });

  it("submits the create product request", async () => {
    const user = userEvent.setup();

    configureQuery({
      data: productListResponse,
    });

    renderPage();

    await user.click(
      screen.getByRole("button", {
        name: "Create product",
      }),
    );

    await user.click(
      screen.getByRole("button", {
        name: "Submit mocked product",
      }),
    );

    await waitFor(() => {
      expect(createMutateAsyncMock).toHaveBeenCalledOnce();
    });

    expect(createMutateAsyncMock).toHaveBeenCalledWith(createProductRequest);
  });

  it("closes the create product dialog and resets the mutation", async () => {
    const user = userEvent.setup();

    configureQuery({
      data: productListResponse,
    });

    renderPage();

    await user.click(
      screen.getByRole("button", {
        name: "Create product",
      }),
    );

    await user.click(
      screen.getByRole("button", {
        name: "Close mocked product",
      }),
    );

    expect(
      screen.queryByRole("dialog", {
        name: "Create product dialog",
      }),
    ).not.toBeInTheDocument();

    expect(createResetMock).toHaveBeenCalledTimes(2);

    expect(createProductDialogMock).toHaveBeenLastCalledWith(
      expect.objectContaining({
        open: false,
      }),
    );
  });

  it("passes the creation error to the dialog", () => {
    const createError = new ApiError(
      409,
      "PRODUCT_SKU_ALREADY_EXISTS",
      "A product with this SKU already exists in the organisation.",
      "conflict-trace-id",
    );

    configureCreateMutation({
      error: createError,
      isError: true,
    });

    configureQuery({
      data: productListResponse,
    });

    renderPage();

    expect(createProductDialogMock).toHaveBeenLastCalledWith(
      expect.objectContaining({
        error: createError,
      }),
    );
  });

  it("passes the pending state to the dialog", () => {
    configureCreateMutation({
      isPending: true,
    });

    configureQuery({
      data: productListResponse,
    });

    renderPage();

    expect(createProductDialogMock).toHaveBeenLastCalledWith(
      expect.objectContaining({
        isPending: true,
      }),
    );
  });

  it("disables creation while a request is running", () => {
    configureCreateMutation({
      isPending: true,
    });

    configureQuery({
      data: productListResponse,
    });

    renderPage();

    expect(
      screen.getByRole("button", {
        name: "Create product",
      }),
    ).toBeDisabled();
  });
});
