import {
  cleanup,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { ProductDetailsPage } from "./ProductDetailsPage";

const useProductMock = vi.hoisted(() => vi.fn());

const useProductStatusActionMock = vi.hoisted(() => vi.fn());

const refetchMock = vi.hoisted(() => vi.fn());
const mutateMock = vi.hoisted(() => vi.fn());
const resetMutationMock = vi.hoisted(() => vi.fn());

vi.mock("../hooks/useProduct", () => ({
  useProduct: useProductMock,
}));

vi.mock("../hooks/useProductStatusAction", () => ({
  useProductStatusAction: useProductStatusActionMock,
}));

const productId = "33333333-3333-3333-3333-333333333333";

const productDetails = {
  productId,
  sku: "CHAIR-001",
  name: "Office Chair",
  description: "Ergonomic office chair.",
  unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
  status: "Active",
  createdAt: "2026-08-27T00:00:00+00:00",
  createdBy: "66666666-6666-6666-6666-666666666666",
  lastModifiedAt: "2026-08-28T01:30:00+00:00",
  lastModifiedBy: "88888888-8888-8888-8888-888888888888",
  rowVersion: "AAAAAAAAB9E=",
};

function configureProductQuery(values = {}) {
  useProductMock.mockReturnValue({
    data: values.data,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isFetching: values.isFetching ?? false,
    refetch: refetchMock,
  });
}

function configureStatusAction(values = {}) {
  useProductStatusActionMock.mockReturnValue({
    mutate: mutateMock,
    reset: resetMutationMock,
    error: values.error ?? null,
    isError: values.isError ?? false,
    isPending: values.isPending ?? false,
  });
}

function renderPage(path = `/products/${productId}`) {
  return render(
    <MemoryRouter initialEntries={[path]}>
      <Routes>
        <Route path="/products" element={<h1>Product list</h1>} />

        <Route path="/products/:productId" element={<ProductDetailsPage />} />

        <Route path="/missing-product-id" element={<ProductDetailsPage />} />
      </Routes>
    </MemoryRouter>,
  );
}

function expectField(label, expectedValue) {
  const labelElement = screen.getByText(label, {
    selector: "dt",
  });

  const valueElement = labelElement.nextElementSibling;

  expect(valueElement).toHaveTextContent(expectedValue);
}

function formatExpectedDate(value) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}

describe("ProductDetailsPage", () => {
  beforeEach(() => {
    useProductMock.mockReset();
    useProductStatusActionMock.mockReset();
    refetchMock.mockReset();
    mutateMock.mockReset();
    resetMutationMock.mockReset();

    configureStatusAction();
  });

  afterEach(() => {
    cleanup();
  });

  it("queries the product ID from the route", () => {
    configureProductQuery({
      data: productDetails,
    });

    renderPage();

    expect(useProductMock).toHaveBeenCalledWith(productId);

    expect(useProductStatusActionMock).toHaveBeenCalledWith(productId);
  });

  it("shows a loading message while details are loading", () => {
    configureProductQuery({
      isPending: true,
      isFetching: true,
    });

    renderPage();

    expect(screen.getByText("Loading product details...")).toBeInTheDocument();

    expect(screen.getByRole("progressbar")).toBeInTheDocument();

    expect(screen.queryByText("Office Chair")).not.toBeInTheDocument();
  });

  it("shows the product details and audit information", () => {
    configureProductQuery({
      data: productDetails,
    });

    renderPage();

    expectField("Product ID", productId);
    expectField("SKU", "CHAIR-001");
    expectField("Name", "Office Chair");

    expectField("Description", "Ergonomic office chair.");

    expectField("Status", "Active");

    expectField("Unit of measure ID", productDetails.unitOfMeasureId);

    expectField("Product category ID", productDetails.productCategoryId);

    expectField("Tax category ID", productDetails.taxCategoryId);

    expectField("Created at", formatExpectedDate(productDetails.createdAt));

    expectField("Created by", productDetails.createdBy);

    expectField(
      "Last modified at",
      formatExpectedDate(productDetails.lastModifiedAt),
    );

    expectField("Last modified by", productDetails.lastModifiedBy);

    expect(
      screen.queryByText(productDetails.rowVersion),
    ).not.toBeInTheDocument();
  });

  it("shows placeholders for optional information", () => {
    configureProductQuery({
      data: {
        ...productDetails,
        description: "",
        productCategoryId: null,
        lastModifiedAt: null,
        lastModifiedBy: null,
      },
    });

    renderPage();

    expectField("Description", "—");
    expectField("Product category ID", "—");
    expectField("Last modified at", "—");
    expectField("Last modified by", "—");
  });

  it("shows a not-found message after a 404", () => {
    configureProductQuery({
      data: productDetails,
      isError: true,
      error: new ApiError(
        404,
        "PRODUCT_NOT_FOUND",
        "The selected product does not exist.",
        "not-found-trace-id",
      ),
    });

    renderPage();

    expect(screen.getByRole("alert")).toHaveTextContent(
      "The selected product does not exist or is not available in your organisation.",
    );

    expect(screen.queryByText("Office Chair")).not.toBeInTheDocument();

    expect(
      screen.queryByRole("button", {
        name: "Retry",
      }),
    ).not.toBeInTheDocument();
  });

  it("shows the API error and retries the request", async () => {
    const user = userEvent.setup();

    configureProductQuery({
      isError: true,
      error: new ApiError(
        500,
        "UNEXPECTED_ERROR",
        "The product service is unavailable.",
        "server-error-trace-id",
      ),
    });

    renderPage();

    expect(screen.getByRole("alert")).toHaveTextContent(
      "The product service is unavailable.",
    );

    await user.click(
      screen.getByRole("button", {
        name: "Retry",
      }),
    );

    expect(refetchMock).toHaveBeenCalledOnce();
  });

  it("shows a generic query error message", () => {
    configureProductQuery({
      isError: true,
      error: new Error("Connection failed."),
    });

    renderPage();

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Unable to load product details.",
    );
  });

  it("disables retry while details are refreshing", () => {
    configureProductQuery({
      isError: true,
      isFetching: true,
      error: new ApiError(
        500,
        "UNEXPECTED_ERROR",
        "The product service is unavailable.",
      ),
    });

    renderPage();

    expect(
      screen.getByRole("button", {
        name: "Retry",
      }),
    ).toBeDisabled();
  });

  it("keeps details visible during background refresh", () => {
    configureProductQuery({
      data: productDetails,
      isFetching: true,
    });

    renderPage();

    expectField("Name", "Office Chair");

    expect(screen.getByRole("progressbar")).toBeInTheDocument();

    expect(
      screen.queryByText("Loading product details..."),
    ).not.toBeInTheDocument();
  });

  it("shows a missing-ID message", () => {
    configureProductQuery({
      isPending: true,
    });

    renderPage("/missing-product-id");

    expect(useProductMock).toHaveBeenCalledWith(undefined);

    expect(useProductStatusActionMock).toHaveBeenCalledWith("");

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Product ID is missing.",
    );

    expect(screen.queryByRole("progressbar")).not.toBeInTheDocument();
  });

  it("returns to the product list", async () => {
    const user = userEvent.setup();

    configureProductQuery({
      data: productDetails,
    });

    renderPage();

    const backLink = screen.getByRole("link", {
      name: "Back to products",
    });

    expect(backLink).toHaveAttribute("href", "/products");

    await user.click(backLink);

    expect(
      await screen.findByRole("heading", {
        name: "Product list",
      }),
    ).toBeInTheDocument();
  });

  it("shows deactivate for an active product", () => {
    configureProductQuery({
      data: productDetails,
    });

    renderPage();

    expect(
      screen.getByRole("button", {
        name: "Deactivate product",
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole("button", {
        name: "Activate product",
      }),
    ).not.toBeInTheDocument();
  });

  it("shows activate for an inactive product", () => {
    configureProductQuery({
      data: {
        ...productDetails,
        status: "Inactive",
      },
    });

    renderPage();

    expect(
      screen.getByRole("button", {
        name: "Activate product",
      }),
    ).toBeInTheDocument();

    expect(
      screen.queryByRole("button", {
        name: "Deactivate product",
      }),
    ).not.toBeInTheDocument();
  });

  it("opens and cancels the confirmation dialog", async () => {
    const user = userEvent.setup();

    configureProductQuery({
      data: productDetails,
    });

    renderPage();

    await user.click(
      screen.getByRole("button", {
        name: "Deactivate product",
      }),
    );

    const dialog = screen.getByRole("dialog");

    expect(
      within(dialog).getByText(/Deactivate Office Chair/),
    ).toBeInTheDocument();

    await user.click(
      within(dialog).getByRole("button", {
        name: "Cancel",
      }),
    );

    await waitFor(() => {
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    });

    expect(mutateMock).not.toHaveBeenCalled();
  });

  it("submits the deactivate action with rowVersion", async () => {
    const user = userEvent.setup();

    configureProductQuery({
      data: productDetails,
    });

    renderPage();

    await user.click(
      screen.getByRole("button", {
        name: "Deactivate product",
      }),
    );

    const dialog = screen.getByRole("dialog");

    await user.click(
      within(dialog).getByRole("button", {
        name: "Deactivate product",
      }),
    );

    expect(mutateMock).toHaveBeenCalledOnce();

    expect(mutateMock).toHaveBeenCalledWith(
      {
        action: "deactivate",
        rowVersion: productDetails.rowVersion,
      },
      expect.objectContaining({
        onSuccess: expect.any(Function),
      }),
    );
  });

  it("submits the activate action with rowVersion", async () => {
    const user = userEvent.setup();

    configureProductQuery({
      data: {
        ...productDetails,
        status: "Inactive",
      },
    });

    renderPage();

    await user.click(
      screen.getByRole("button", {
        name: "Activate product",
      }),
    );

    const dialog = screen.getByRole("dialog");

    await user.click(
      within(dialog).getByRole("button", {
        name: "Activate product",
      }),
    );

    expect(mutateMock).toHaveBeenCalledWith(
      {
        action: "activate",
        rowVersion: productDetails.rowVersion,
      },
      expect.objectContaining({
        onSuccess: expect.any(Function),
      }),
    );
  });

  it("closes the dialog after a successful action", async () => {
    const user = userEvent.setup();

    mutateMock.mockImplementation((_parameters, options) => {
      options.onSuccess();
    });

    configureProductQuery({
      data: productDetails,
    });

    renderPage();

    await user.click(
      screen.getByRole("button", {
        name: "Deactivate product",
      }),
    );

    const dialog = screen.getByRole("dialog");

    await user.click(
      within(dialog).getByRole("button", {
        name: "Deactivate product",
      }),
    );

    await waitFor(() => {
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    });
  });

  it("disables the status button while a request is pending", () => {
    configureProductQuery({
      data: productDetails,
    });

    configureStatusAction({
      isPending: true,
    });

    renderPage();

    expect(
      screen.getByRole("button", {
        name: "Deactivate product",
      }),
    ).toBeDisabled();
  });

  it("shows a concurrency error and reloads the product", async () => {
    const user = userEvent.setup();

    configureProductQuery({
      data: productDetails,
    });

    configureStatusAction({
      isError: true,
      error: new ApiError(
        409,
        "PRODUCT_CONCURRENCY_CONFLICT",
        "The product was changed by another request.",
        "conflict-trace-id",
      ),
    });

    renderPage();

    await user.click(
      screen.getByRole("button", {
        name: "Deactivate product",
      }),
    );

    const dialog = screen.getByRole("dialog");

    expect(within(dialog).getByRole("alert")).toHaveTextContent(
      "The product was changed by another request.",
    );

    await user.click(
      within(dialog).getByRole("button", {
        name: "Reload product",
      }),
    );

    expect(refetchMock).toHaveBeenCalledOnce();

    await waitFor(() => {
      expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
    });
  });

  it("shows a normal status action API error", async () => {
    const user = userEvent.setup();

    configureProductQuery({
      data: productDetails,
    });

    configureStatusAction({
      isError: true,
      error: new ApiError(
        500,
        "UNEXPECTED_ERROR",
        "Unable to contact the product service.",
      ),
    });

    renderPage();

    await user.click(
      screen.getByRole("button", {
        name: "Deactivate product",
      }),
    );

    const dialog = screen.getByRole("dialog");

    expect(within(dialog).getByRole("alert")).toHaveTextContent(
      "Unable to contact the product service.",
    );

    expect(
      within(dialog).queryByRole("button", {
        name: "Reload product",
      }),
    ).not.toBeInTheDocument();
  });
});
