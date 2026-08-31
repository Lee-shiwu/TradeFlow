/** @typedef {import('../api/productDetailsTypes.js').ProductDetails} ProductDetails */

/** @typedef {import('../hooks/useProduct.js').useProduct} useProduct */

import { cleanup, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { ProductDetailsPage } from "./ProductDetailsPage";
const useProductMock = vi.hoisted(() => vi.fn());
const refetchMock = vi.hoisted(() => vi.fn());
vi.mock("../hooks/useProduct", () => ({
  useProduct: useProductMock,
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
function configureQuery(values = {}) {
  useProductMock.mockReturnValue({
    data: values.data,
    error: values.error ?? null,
    isPending: values.isPending ?? false,
    isError: values.isError ?? false,
    isFetching: values.isFetching ?? false,
    refetch: refetchMock,
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
    refetchMock.mockReset();
  });
  afterEach(() => {
    cleanup();
  });
  it("queries the product ID from the route", () => {
    configureQuery({
      data: productDetails,
    });
    renderPage();
    expect(useProductMock).toHaveBeenLastCalledWith(productId);
  });
  it("shows a loading message while details are loading", () => {
    configureQuery({
      isPending: true,
      isFetching: true,
    });
    renderPage();
    expect(screen.getByText("Loading product details...")).toBeInTheDocument();
    expect(screen.getByRole("progressbar")).toBeInTheDocument();
    expect(screen.queryByText("Office Chair")).not.toBeInTheDocument();
  });
  it("shows the product details and audit information", () => {
    configureQuery({
      data: productDetails,
    });
    renderPage();
    expectField("Product ID", productId);
    expectField("SKU", "CHAIR-001");
    expectField("Name", "Office Chair");
    expectField("Description", "Ergonomic office chair.");
    expectField("Status", "Active");
    expectField("Unit of measure ID", productDetails.unitOfMeasureId);
    expectField("Product category ID", "77777777-7777-7777-7777-777777777777");
    expectField("Tax category ID", productDetails.taxCategoryId);
    expectField("Created at", formatExpectedDate(productDetails.createdAt));
    expectField("Created by", productDetails.createdBy);
    expectField(
      "Last modified at",
      formatExpectedDate("2026-08-28T01:30:00+00:00"),
    );
    expectField("Last modified by", "88888888-8888-8888-8888-888888888888");
    expect(
      screen.queryByText(productDetails.rowVersion),
    ).not.toBeInTheDocument();
  });
  it("shows placeholders for empty optional information", () => {
    configureQuery({
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
  it("shows a not-found message and hides stale details after a 404", () => {
    configureQuery({
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
      screen.queryByRole("button", { name: "Retry" }),
    ).not.toBeInTheDocument();
  });
  it("shows the API error and retries the request", async () => {
    const user = userEvent.setup();
    configureQuery({
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
    await user.click(screen.getByRole("button", { name: "Retry" }));
    expect(refetchMock).toHaveBeenCalledOnce();
  });
  it("shows a generic message for an unknown error", () => {
    configureQuery({
      isError: true,
      error: new Error("Connection failed."),
    });
    renderPage();
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Unable to load product details.",
    );
  });
  it("disables retry while a request is running", () => {
    configureQuery({
      isError: true,
      isFetching: true,
      error: new ApiError(
        500,
        "UNEXPECTED_ERROR",
        "The product service is unavailable.",
      ),
    });
    renderPage();
    const retryButton = screen.getByRole("button", {
      name: "Retry",
    });
    expect(retryButton).toBeDisabled();
    expect(refetchMock).not.toHaveBeenCalled();
  });
  it("keeps product details visible during a background refresh", () => {
    configureQuery({
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
  it("shows a missing-ID message instead of an endless loading state", () => {
    configureQuery({
      isPending: true,
      isFetching: false,
    });
    renderPage("/missing-product-id");
    expect(useProductMock).toHaveBeenLastCalledWith(undefined);
    expect(screen.getByRole("alert")).toHaveTextContent(
      "Product ID is missing.",
    );
    expect(screen.queryByRole("progressbar")).not.toBeInTheDocument();
  });
  it("returns to the product list when the back link is clicked", async () => {
    const user = userEvent.setup();
    configureQuery({
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
});
