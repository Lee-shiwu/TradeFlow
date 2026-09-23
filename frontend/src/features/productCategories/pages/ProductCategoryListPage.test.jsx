import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { ApiError } from "../../../shared/api/ApiError";
import { ProductCategoryListPage } from "./ProductCategoryListPage";

const useProductCategoriesMock = vi.hoisted(() => vi.fn());
const refetchMock = vi.hoisted(() => vi.fn());

vi.mock("../hooks/useProductCategories", () => ({
  useProductCategories: useProductCategoriesMock,
}));

const response = {
  items: [
    {
      productCategoryId: "77777777-7777-7777-7777-777777777777",
      code: "OFFICE",
      name: "Office products",
      description: "Products used in the office.",
      status: "Active",
      createdAt: "2026-09-01T00:00:00+00:00",
      lastModifiedAt: null,
    },
  ],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 1,
  totalPages: 1,
};

function configureQuery(values = {}) {
  useProductCategoriesMock.mockReturnValue({
    data: values.data,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isFetching: values.isFetching ?? false,
    refetch: refetchMock,
  });
}

function renderPage() {
  render(
    <MemoryRouter initialEntries={["/product-categories"]}>
      <Routes>
        <Route
          path="/product-categories"
          element={<ProductCategoryListPage />}
        />
        <Route
          path="/product-categories/:productCategoryId"
          element={<div>Product category details destination</div>}
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe("ProductCategoryListPage", () => {
  beforeEach(() => {
    useProductCategoriesMock.mockReset();
    refetchMock.mockReset();
  });

  afterEach(cleanup);

  it("shows loading state", () => {
    configureQuery({ isPending: true, isFetching: true });
    renderPage();

    expect(
      screen.getByText("Loading product categories..."),
    ).toBeInTheDocument();
  });

  it("shows product categories", () => {
    configureQuery({ data: response });
    renderPage();

    expect(screen.getByText("Total product categories: 1")).toBeInTheDocument();
    expect(screen.getByText("OFFICE")).toBeInTheDocument();
    expect(screen.getByText("Office products")).toBeInTheDocument();
    expect(
      screen.getByText("Products used in the office."),
    ).toBeInTheDocument();
    expect(screen.getByText("Active")).toBeInTheDocument();
  });

  it("shows an empty state", () => {
    configureQuery({
      data: { ...response, items: [], totalCount: 0, totalPages: 0 },
    });
    renderPage();

    expect(
      screen.getByText("No product categories found."),
    ).toBeInTheDocument();
    expect(screen.queryByRole("table")).not.toBeInTheDocument();
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

  it("applies trimmed search and resets to page one", async () => {
    const user = userEvent.setup();
    configureQuery({ data: response });
    renderPage();

    await user.type(screen.getByLabelText("Search"), "  office  ");
    await user.click(screen.getByRole("button", { name: "Search" }));

    await waitFor(() => {
      expect(useProductCategoriesMock).toHaveBeenLastCalledWith({
        search: "office",
        status: undefined,
        pageNumber: 1,
        pageSize: 20,
      });
    });
  });

  it("applies the selected status", async () => {
    const user = userEvent.setup();
    configureQuery({ data: response });
    renderPage();

    await user.click(screen.getByLabelText("Status"));
    await user.click(screen.getByRole("option", { name: "Inactive" }));

    await waitFor(() => {
      expect(useProductCategoriesMock).toHaveBeenLastCalledWith({
        search: "",
        status: "Inactive",
        pageNumber: 1,
        pageSize: 20,
      });
    });
  });

  it("requests the selected page", async () => {
    const user = userEvent.setup();
    configureQuery({ data: { ...response, totalCount: 60, totalPages: 3 } });
    renderPage();

    await user.click(screen.getByRole("button", { name: "Go to page 2" }));

    await waitFor(() => {
      expect(useProductCategoriesMock).toHaveBeenLastCalledWith({
        search: "",
        status: undefined,
        pageNumber: 2,
        pageSize: 20,
      });
    });
  });

  it("opens the selected category details", async () => {
    const user = userEvent.setup();
    configureQuery({ data: response });
    renderPage();

    await user.click(screen.getByRole("link", { name: "View details" }));

    expect(
      screen.getByText("Product category details destination"),
    ).toBeInTheDocument();
  });
});
