import { cleanup, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { ApiError } from "../../../shared/api/ApiError";
import { TaxCategoryDetailsPage } from "./TaxCategoryDetailsPage";

const useTaxCategoryMock = vi.hoisted(() => vi.fn());
const refetchMock = vi.hoisted(() => vi.fn());

vi.mock("../hooks/useTaxCategory", () => ({
  useTaxCategory: useTaxCategoryMock,
}));

const taxCategoryId = "20000000-0000-0000-0000-000000000001";
const details = {
  taxCategoryId,
  code: "GST15",
  name: "Standard GST",
  description: "Standard-rated supplies at 15% GST.",
  rate: 0.15,
  treatment: "StandardRated",
  status: "Active",
  effectiveFrom: "2010-10-01",
  effectiveTo: null,
  rowVersion: "AAAAAAAAB9E=",
};

function configureQuery(values = {}) {
  useTaxCategoryMock.mockReturnValue({
    data: values.data,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isFetching: values.isFetching ?? false,
    refetch: refetchMock,
  });
}

function renderPage(initialEntry = `/tax-categories/${taxCategoryId}`) {
  return render(
    <MemoryRouter initialEntries={[initialEntry]}>
      <Routes>
        <Route
          path="/tax-categories/:taxCategoryId"
          element={<TaxCategoryDetailsPage />}
        />
        <Route
          path="/missing-tax-category-id"
          element={<TaxCategoryDetailsPage />}
        />
        <Route
          path="/tax-categories"
          element={<div>Tax category list destination</div>}
        />
      </Routes>
    </MemoryRouter>,
  );
}

describe("TaxCategoryDetailsPage", () => {
  beforeEach(() => {
    useTaxCategoryMock.mockReset();
    refetchMock.mockReset();
  });

  afterEach(cleanup);

  it("shows the loading state", () => {
    configureQuery({ isPending: true, isFetching: true });
    renderPage();

    expect(
      screen.getByText("Loading tax category details..."),
    ).toBeInTheDocument();
    expect(useTaxCategoryMock).toHaveBeenCalledWith(taxCategoryId);
  });

  it("shows tax category details", () => {
    configureQuery({ data: details });
    renderPage();

    expect(screen.getByText("GST15")).toBeInTheDocument();
    expect(screen.getByText("Standard GST")).toBeInTheDocument();
    expect(screen.getByText("Standard rated")).toBeInTheDocument();
    expect(screen.getByText("15%")).toBeInTheDocument();
    expect(screen.getByText("Active")).toBeInTheDocument();
    expect(screen.getByText("No end date")).toBeInTheDocument();
  });

  it("shows a not-found message without retrying", () => {
    configureQuery({
      error: new ApiError(
        404,
        "TAX_CATEGORY_NOT_FOUND",
        "The requested tax category was not found.",
      ),
      isError: true,
    });
    renderPage();

    expect(screen.getByText(/does not exist/i)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Retry" })).toBeNull();
  });

  it("shows an API error and retries", async () => {
    const user = userEvent.setup();
    configureQuery({
      error: new ApiError(500, "UNEXPECTED_ERROR", "Tax service failed."),
      isError: true,
    });
    renderPage();

    expect(screen.getByText("Tax service failed.")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Retry" }));
    expect(refetchMock).toHaveBeenCalledOnce();
  });

  it("shows a missing ID message without requesting details", () => {
    configureQuery();
    renderPage("/missing-tax-category-id");

    expect(screen.getByText("Tax category ID is missing.")).toBeInTheDocument();
    expect(useTaxCategoryMock).toHaveBeenCalledWith(undefined);
  });

  it("returns to the tax category list", async () => {
    const user = userEvent.setup();
    configureQuery({ data: details });
    renderPage();

    await user.click(
      screen.getByRole("link", { name: "Back to tax categories" }),
    );

    expect(
      screen.getByText("Tax category list destination"),
    ).toBeInTheDocument();
  });
});
