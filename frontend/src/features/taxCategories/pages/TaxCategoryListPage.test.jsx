import { cleanup, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { TaxCategoryListPage } from "./TaxCategoryListPage";

const useTaxCategoriesMock = vi.hoisted(() => vi.fn());
const refetchMock = vi.hoisted(() => vi.fn());

vi.mock("../hooks/useTaxCategories", () => ({
  useTaxCategories: useTaxCategoriesMock,
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

describe("TaxCategoryListPage", () => {
  beforeEach(() => {
    useTaxCategoriesMock.mockReset();
    refetchMock.mockReset();
  });

  afterEach(cleanup);

  it("shows loading and empty states", () => {
    configureQuery({ isPending: true });
    const { rerender } = render(<TaxCategoryListPage />);
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
    rerender(<TaxCategoryListPage />);
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
    render(<TaxCategoryListPage />);

    expect(screen.getByText("GST15")).toBeInTheDocument();
    expect(screen.getByText("Standard GST")).toBeInTheDocument();
    expect(screen.getByText("Standard rated")).toBeInTheDocument();
    expect(screen.getByText("15%")).toBeInTheDocument();
    expect(screen.getByText("Active")).toBeInTheDocument();
    expect(screen.getByText("No end date")).toBeInTheDocument();
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
    render(<TaxCategoryListPage />);

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
    const { rerender } = render(<TaxCategoryListPage />);
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
    rerender(<TaxCategoryListPage />);
    expect(screen.getByRole("button", { name: "page 2" })).toBeDisabled();
  });

  it("shows API errors and retries", async () => {
    const user = userEvent.setup();
    configureQuery({
      error: new ApiError(500, "UNEXPECTED_ERROR", "Tax service failed."),
      isError: true,
    });
    render(<TaxCategoryListPage />);

    expect(screen.getByText("Tax service failed.")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Retry" }));
    expect(refetchMock).toHaveBeenCalledOnce();
  });
});
