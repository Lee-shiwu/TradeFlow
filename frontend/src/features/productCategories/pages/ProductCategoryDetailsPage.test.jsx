import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { ApiError } from "../../../shared/api/ApiError";
import { ProductCategoryDetailsPage } from "./ProductCategoryDetailsPage";

const useProductCategoryMock = vi.hoisted(() => vi.fn());
const refetchMock = vi.hoisted(() => vi.fn());
const useUpdateProductCategoryDetailsMock = vi.hoisted(() => vi.fn());
const mutateAsyncMock = vi.hoisted(() => vi.fn());
const resetMutationMock = vi.hoisted(() => vi.fn());

vi.mock("../hooks/useProductCategory", () => ({
  useProductCategory: useProductCategoryMock,
}));

vi.mock("../hooks/useUpdateProductCategoryDetails", () => ({
  useUpdateProductCategoryDetails: useUpdateProductCategoryDetailsMock,
}));

const productCategoryId = "77777777-7777-7777-7777-777777777777";

const details = {
  productCategoryId,
  code: "OFFICE",
  name: "Office products",
  description: "Products used in the office.",
  status: "Active",
  createdAt: "2026-09-20T10:00:00+00:00",
  createdBy: "11111111-1111-1111-1111-111111111111",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

function configureQuery(values = {}) {
  useProductCategoryMock.mockReturnValue({
    data: values.data,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isFetching: values.isFetching ?? false,
    refetch: refetchMock,
  });
}

function renderPage(initialEntry = `/product-categories/${productCategoryId}`) {
  render(
    <MemoryRouter initialEntries={[initialEntry]}>
      <Routes>
        <Route
          path="/product-categories/:productCategoryId"
          element={<ProductCategoryDetailsPage />}
        />
        <Route
          path="/missing-product-category-id"
          element={<ProductCategoryDetailsPage />}
        />
        <Route
          path="/product-categories"
          element={<div>Product category list destination</div>}
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe("ProductCategoryDetailsPage", () => {
  beforeEach(() => {
    useProductCategoryMock.mockReset();
    refetchMock.mockReset();
    useUpdateProductCategoryDetailsMock.mockReset();
    mutateAsyncMock.mockReset();
    resetMutationMock.mockReset();
    mutateAsyncMock.mockResolvedValue(undefined);
    useUpdateProductCategoryDetailsMock.mockReturnValue({
      isPending: false,
      error: null,
      mutateAsync: mutateAsyncMock,
      reset: resetMutationMock,
    });
  });

  afterEach(cleanup);

  it("shows loading state", () => {
    configureQuery({ isPending: true, isFetching: true });
    renderPage();

    expect(
      screen.getByText("Loading product category details..."),
    ).toBeInTheDocument();
    expect(useProductCategoryMock).toHaveBeenCalledWith(productCategoryId);
  });

  it("shows category details", () => {
    configureQuery({ data: details });
    renderPage();

    expect(screen.getByText("OFFICE")).toBeInTheDocument();
    expect(screen.getByText("Office products")).toBeInTheDocument();
    expect(
      screen.getByText("Products used in the office."),
    ).toBeInTheDocument();
    expect(screen.getByText("Active")).toBeInTheDocument();
    expect(screen.getByText(details.createdBy)).toBeInTheDocument();
    expect(screen.getAllByText("—")).toHaveLength(2);
  });

  it("shows a not-found message", () => {
    configureQuery({
      error: new ApiError(
        404,
        "PRODUCT_CATEGORY_NOT_FOUND",
        "The requested product category was not found.",
      ),
      isError: true,
    });
    renderPage();

    expect(
      screen.getByText(/does not exist or is not available/i),
    ).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Retry" })).toBeNull();
  });

  it("shows an API error and retries", async () => {
    const user = userEvent.setup();
    configureQuery({
      error: new ApiError(500, "UNEXPECTED_ERROR", "Category service failed."),
      isError: true,
    });
    renderPage();

    expect(screen.getByText("Category service failed.")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Retry" }));
    expect(refetchMock).toHaveBeenCalledOnce();
  });

  it("shows a missing ID message without requesting details", () => {
    configureQuery();
    renderPage("/missing-product-category-id");

    expect(
      screen.getByText("Product category ID is missing."),
    ).toBeInTheDocument();
    expect(useProductCategoryMock).toHaveBeenCalledWith(undefined);
  });

  it("returns to the category list", async () => {
    const user = userEvent.setup();
    configureQuery({ data: details });
    renderPage();

    await user.click(
      screen.getByRole("link", { name: "Back to product categories" }),
    );

    expect(
      screen.getByText("Product category list destination"),
    ).toBeInTheDocument();
  });

  it("opens the editor and submits category details", async () => {
    const user = userEvent.setup();
    configureQuery({ data: details });
    renderPage();

    await user.click(screen.getByRole("button", { name: "Edit category" }));
    await user.clear(screen.getByLabelText(/Category name/));
    await user.type(screen.getByLabelText(/Category name/), "Updated category");
    await user.click(screen.getByRole("button", { name: "Save changes" }));

    expect(mutateAsyncMock).toHaveBeenCalledWith({
      name: "Updated category",
      description: details.description,
      rowVersion: details.rowVersion,
    });
    await waitFor(() => {
      expect(
        screen.queryByRole("dialog", { name: "Edit product category" }),
      ).toBeNull();
    });
  });

  it("reloads details after an edit conflict", async () => {
    const user = userEvent.setup();
    configureQuery({ data: details });
    useUpdateProductCategoryDetailsMock.mockReturnValue({
      isPending: false,
      error: new ApiError(
        409,
        "UPDATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT",
        "Category changed.",
      ),
      mutateAsync: mutateAsyncMock,
      reset: resetMutationMock,
    });
    renderPage();

    await user.click(screen.getByRole("button", { name: "Edit category" }));
    await user.click(screen.getByRole("button", { name: "Reload category" }));

    expect(refetchMock).toHaveBeenCalledOnce();
    expect(resetMutationMock).toHaveBeenCalled();
  });
});
