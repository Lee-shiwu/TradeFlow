import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { TaxCategoryListPage } from "./TaxCategoryListPage";
import { MemoryRouter } from "react-router-dom";

const useTaxCategoriesMock = vi.hoisted(() => vi.fn());
const useCreateTaxCategoryMock = vi.hoisted(() => vi.fn());
const refetchMock = vi.hoisted(() => vi.fn());
const createMutateAsyncMock = vi.hoisted(() => vi.fn());
const createResetMock = vi.hoisted(() => vi.fn());

vi.mock("../hooks/useTaxCategories", () => ({
  useTaxCategories: useTaxCategoriesMock,
}));

vi.mock("../hooks/useCreateTaxCategory", () => ({
  useCreateTaxCategory: useCreateTaxCategoryMock,
}));

const items = [
  {
    taxCategoryId: "20000000-0000-0000-0000-000000000001",
    code: "GST15",
    name: "Standard GST",
    description: "Standard-rated supplies at 15% GST.",
    rate: 0.15,
    treatment: "StandardRated",
    status: "Active",
    effectiveFrom: "2010-10-01",
    effectiveTo: null,
  },
];

function configureQuery(values = {}) {
  useTaxCategoriesMock.mockReturnValue({
    data: values.data,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isFetching: values.isFetching ?? false,
    refetch: refetchMock,
  });
}

function pageElement() {
  return (
    <MemoryRouter>
      <TaxCategoryListPage />
    </MemoryRouter>
  );
}

describe("TaxCategoryListPage", () => {
  beforeEach(() => {
    useTaxCategoriesMock.mockReset();
    useCreateTaxCategoryMock.mockReset();
    refetchMock.mockReset();
    createMutateAsyncMock.mockReset();
    createResetMock.mockReset();
    useCreateTaxCategoryMock.mockReturnValue({
      error: null,
      isPending: false,
      mutateAsync: createMutateAsyncMock,
      reset: createResetMock,
    });
  });

  afterEach(cleanup);

  it("shows loading and empty states", () => {
    configureQuery({ isPending: true });
    const { rerender } = render(pageElement());
    expect(screen.getByText("Loading tax categories...")).toBeInTheDocument();

    configureQuery({
      data: {
        items: [],
        pageNumber: 1,
        pageSize: 20,
        totalCount: 0,
        totalPages: 0,
      },
    });
    rerender(pageElement());
    expect(screen.getByText("No tax categories found.")).toBeInTheDocument();
  });

  it("renders treatment, percentage, status, and effective dates", () => {
    configureQuery({
      data: {
        items,
        pageNumber: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
      },
    });
    render(pageElement());

    expect(screen.getByText("GST15")).toBeInTheDocument();
    expect(screen.getByText("Standard GST")).toBeInTheDocument();
    expect(screen.getByText("Standard rated")).toBeInTheDocument();
    expect(screen.getByText("15%")).toBeInTheDocument();
    expect(screen.getByText("Active")).toBeInTheDocument();
    expect(screen.getByText("No end date")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "View details" })).toHaveAttribute(
      "href",
      `/tax-categories/${items[0].taxCategoryId}`,
    );
  });

  it("applies search and status filters", async () => {
    const user = userEvent.setup();
    configureQuery({
      data: {
        items,
        pageNumber: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
      },
    });
    render(pageElement());

    await user.type(screen.getByLabelText("Search"), " gst ");
    await user.click(screen.getByLabelText("Status"));
    await user.click(screen.getByRole("option", { name: "Active" }));
    await user.click(screen.getByRole("button", { name: "Search" }));

    expect(useTaxCategoriesMock).toHaveBeenLastCalledWith({
      search: "gst",
      status: "Active",
      pageNumber: 1,
      pageSize: 20,
    });
  });

  it("changes pages and disables pagination while fetching", async () => {
    const user = userEvent.setup();
    configureQuery({
      data: {
        items,
        pageNumber: 1,
        pageSize: 20,
        totalCount: 25,
        totalPages: 2,
      },
    });
    const { rerender } = render(pageElement());
    await user.click(screen.getByRole("button", { name: "Go to page 2" }));
    expect(useTaxCategoriesMock).toHaveBeenLastCalledWith(
      expect.objectContaining({ pageNumber: 2 }),
    );

    configureQuery({
      data: {
        items,
        pageNumber: 2,
        pageSize: 20,
        totalCount: 25,
        totalPages: 2,
      },
      isFetching: true,
    });
    rerender(pageElement());
    expect(screen.getByRole("button", { name: "page 2" })).toBeDisabled();
  });

  it("shows API errors and retries", async () => {
    const user = userEvent.setup();
    configureQuery({
      error: new ApiError(500, "UNEXPECTED_ERROR", "Tax service failed."),
      isError: true,
    });
    render(pageElement());

    expect(screen.getByText("Tax service failed.")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Retry" }));
    expect(refetchMock).toHaveBeenCalledOnce();
  });

  it("creates a tax category from the list page", async () => {
    const user = userEvent.setup();
    createMutateAsyncMock.mockResolvedValue({
      taxCategoryId: "tax-category-id",
      code: "REGIONAL",
      name: "Regional levy",
      description: "",
      rate: 0.125,
      treatment: "StandardRated",
      status: "Active",
      effectiveFrom: "2026-10-01",
      effectiveTo: null,
      rowVersion: "AAAAAAAAB9E=",
    });
    configureQuery({
      data: {
        items,
        pageNumber: 1,
        pageSize: 20,
        totalCount: 1,
        totalPages: 1,
      },
    });
    render(pageElement());

    await user.click(
      screen.getByRole("button", { name: "Create tax category" }),
    );
    await user.type(screen.getByLabelText(/Tax category code/), "regional");
    await user.type(
      screen.getByLabelText(/Tax category name/),
      "Regional levy",
    );
    await user.type(screen.getByLabelText(/Rate/), "12.5");
    await user.type(screen.getByLabelText(/Effective from/), "2026-10-01");
    await user.click(
      screen.getByRole("button", { name: "Create tax category" }),
    );

    await waitFor(() => {
      expect(createMutateAsyncMock).toHaveBeenCalledWith({
        code: "REGIONAL",
        name: "Regional levy",
        description: null,
        rate: 0.125,
        treatment: "StandardRated",
        effectiveFrom: "2026-10-01",
        effectiveTo: null,
      });
    });
  });
});
