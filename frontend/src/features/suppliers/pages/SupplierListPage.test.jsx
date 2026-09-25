import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { SupplierListPage } from "./SupplierListPage";

const useSuppliersMock = vi.hoisted(() => vi.fn());
const useCreateSupplierMock = vi.hoisted(() => vi.fn());
const refetchMock = vi.hoisted(() => vi.fn());
const mutateAsyncMock = vi.hoisted(() => vi.fn());
const resetMock = vi.hoisted(() => vi.fn());

vi.mock("../hooks/useSuppliers", () => ({ useSuppliers: useSuppliersMock }));
vi.mock("../hooks/useCreateSupplier", () => ({
  useCreateSupplier: useCreateSupplierMock,
}));

const response = {
  items: [
    {
      supplierId: "supplier-id",
      code: "OFFICE-01",
      name: "Auckland Office Supplies",
      status: "Active",
      createdAt: "2026-09-25T09:00:00+00:00",
    },
  ],
  pageNumber: 1,
  pageSize: 20,
  totalCount: 1,
  totalPages: 1,
};

function configureQuery(values = {}) {
  useSuppliersMock.mockReturnValue({
    data: values.data,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isFetching: values.isFetching ?? false,
    refetch: refetchMock,
  });
}

describe("SupplierListPage", () => {
  beforeEach(() => {
    useSuppliersMock.mockReset();
    useCreateSupplierMock.mockReset();
    refetchMock.mockReset();
    mutateAsyncMock.mockReset();
    resetMock.mockReset();
    useCreateSupplierMock.mockReturnValue({
      error: null,
      isPending: false,
      mutateAsync: mutateAsyncMock,
      reset: resetMock,
    });
  });

  afterEach(cleanup);

  it("shows loading, empty, and populated states", () => {
    configureQuery({ isPending: true });
    const { rerender } = render(<SupplierListPage />);
    expect(screen.getByText("Loading suppliers...")).toBeInTheDocument();

    configureQuery({ data: { ...response, items: [], totalCount: 0 } });
    rerender(<SupplierListPage />);
    expect(screen.getByText("No suppliers found.")).toBeInTheDocument();

    configureQuery({ data: response });
    rerender(<SupplierListPage />);
    expect(screen.getByText("OFFICE-01")).toBeInTheDocument();
    expect(screen.getByText("Auckland Office Supplies")).toBeInTheDocument();
  });

  it("applies search and status filters", async () => {
    const user = userEvent.setup();
    configureQuery({ data: response });
    render(<SupplierListPage />);

    await user.type(screen.getByLabelText("Search"), " office ");
    await user.click(screen.getByLabelText("Status"));
    await user.click(screen.getByRole("option", { name: "Inactive" }));
    await user.click(screen.getByRole("button", { name: "Search" }));

    expect(useSuppliersMock).toHaveBeenLastCalledWith({
      search: "office",
      status: "Inactive",
      pageNumber: 1,
      pageSize: 20,
    });
  });

  it("shows API errors and retries", async () => {
    const user = userEvent.setup();
    configureQuery({
      error: new ApiError(500, "UNEXPECTED_ERROR", "Supplier service failed."),
      isError: true,
    });
    render(<SupplierListPage />);

    expect(screen.getByText("Supplier service failed.")).toBeInTheDocument();
    await user.click(screen.getByRole("button", { name: "Retry" }));
    expect(refetchMock).toHaveBeenCalledOnce();
  });

  it("creates a supplier from the list page", async () => {
    const user = userEvent.setup();
    mutateAsyncMock.mockResolvedValue({ supplierId: "new-supplier-id" });
    configureQuery({ data: response });
    render(<SupplierListPage />);

    await user.click(screen.getByRole("button", { name: "Create supplier" }));
    await user.type(screen.getByLabelText(/Supplier code/), "supplier-02");
    await user.type(screen.getByLabelText(/Supplier name/), "Second supplier");
    await user.click(screen.getByRole("button", { name: "Create supplier" }));

    await waitFor(() => {
      expect(mutateAsyncMock).toHaveBeenCalledWith({
        code: "SUPPLIER-02",
        name: "Second supplier",
      });
    });
  });
});
