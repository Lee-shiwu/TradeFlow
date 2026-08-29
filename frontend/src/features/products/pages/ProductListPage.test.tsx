import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import type { ListProductsResponse } from "../api/productListTypes";
import { useProducts } from "../hooks/useProducts";
import { ProductListPage } from "./ProductListPage";

const useProductsMock = vi.hoisted(() => vi.fn());
const refetchMock = vi.hoisted(() => vi.fn());

vi.mock("../hooks/useProducts", () => ({
  useProducts: useProductsMock,
}));

const productListResponse: ListProductsResponse = {
  items: [
    {
      productId: "33333333-3333-3333-3333-333333333333",
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
  totalPages: 3,
};

function configureQuery(
  values: {
    data?: ListProductsResponse;
    error?: unknown;
    isPending?: boolean;
    isError?: boolean;
    isFetching?: boolean;
  } = {},
) {
  useProductsMock.mockReturnValue({
    data: values.data,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isFetching: values.isFetching ?? false,
    refetch: refetchMock,
  } as unknown as ReturnType<typeof useProducts>);
}

describe("ProductListPage", () => {
  beforeEach(() => {
    useProductsMock.mockReset();
    refetchMock.mockReset();
  });

  afterEach(() => {
    cleanup();
  });

  it("shows a loading message while products are loading", () => {
    configureQuery({
      isPending: true,
      isFetching: true,
    });

    render(<ProductListPage />);

    expect(screen.getByText("Loading products...")).toBeInTheDocument();
  });

  it("shows products in the product table", () => {
    configureQuery({
      data: productListResponse,
    });

    render(<ProductListPage />);

    expect(screen.getByText("Total products: 1")).toBeInTheDocument();

    expect(screen.getByText("CHAIR-001")).toBeInTheDocument();

    expect(screen.getByText("Office Chair")).toBeInTheDocument();

    expect(screen.getByText("Active")).toBeInTheDocument();
  });

  it("shows an empty message when no products exist", () => {
    configureQuery({
      data: {
        items: [],
        pageNumber: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      },
    });

    render(<ProductListPage />);

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

    render(<ProductListPage />);

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

  it("applies the entered product search", async () => {
    const user = userEvent.setup();

    configureQuery({
      data: {
        items: [],
        pageNumber: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      },
    });

    render(<ProductListPage />);

    await user.type(
      screen.getByRole("textbox", {
        name: "Search",
      }),
      "  office chair  ",
    );

    await user.click(
      screen.getByRole("button", {
        name: "Search",
      }),
    );

    await waitFor(() => {
      expect(useProductsMock).toHaveBeenLastCalledWith({
        search: "office chair",
        status: undefined,
        pageNumber: 1,
        pageSize: 20,
      });
    });
  });

  it("applies the selected product status", async () => {
    const user = userEvent.setup();

    configureQuery({
      data: {
        items: [],
        pageNumber: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      },
    });

    render(<ProductListPage />);

    await user.click(
      screen.getByRole("combobox", {
        name: "Status",
      }),
    );

    await user.click(
      screen.getByRole("option", {
        name: "Inactive",
      }),
    );

    await waitFor(() => {
      expect(useProductsMock).toHaveBeenLastCalledWith({
        search: "",
        status: "Inactive",
        pageNumber: 1,
        pageSize: 20,
      });
    });
  });

  it("requests the selected product page", async () => {
    const user = userEvent.setup();

    configureQuery({
      data: productListResponse,
    });

    render(<ProductListPage />);

    await user.click(
      screen.getByRole("button", {
        name: "Go to page 2",
      }),
    );

    await waitFor(() => {
      expect(useProductsMock).toHaveBeenLastCalledWith({
        search: "",
        status: undefined,
        pageNumber: 2,
        pageSize: 20,
      });
    });
  });
});
