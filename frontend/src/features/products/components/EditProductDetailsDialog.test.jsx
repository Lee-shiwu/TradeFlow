import {
  cleanup,
  fireEvent,
  render,
  screen,
  waitFor,
} from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { EditProductDetailsDialog } from "./EditProductDetailsDialog";

const productCategoryId = "77777777-7777-7777-7777-777777777777";
const otherProductCategoryId = "88888888-8888-8888-8888-888888888888";
const taxCategoryId = "55555555-5555-5555-5555-555555555555";
const otherTaxCategoryId = "99999999-9999-9999-9999-999999999999";

const product = {
  productId: "33333333-3333-3333-3333-333333333333",
  sku: "CHAIR-001",
  name: "Office Chair",
  description: "Ergonomic office chair.",
  unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
  productCategoryId,
  taxCategoryId,
  status: "Active",
  createdAt: "2026-08-27T00:00:00+00:00",
  createdBy: "66666666-6666-6666-6666-666666666666",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

const referenceData = {
  unitsOfMeasure: [],
  productCategories: [
    { id: productCategoryId, code: "OFFICE", name: "Office products" },
    { id: otherProductCategoryId, code: "FURN", name: "Furniture" },
  ],
  taxCategories: [
    { id: taxCategoryId, code: "GST15", name: "Standard GST" },
    { id: otherTaxCategoryId, code: "ZERO", name: "Zero rated" },
  ],
};

const onClose = vi.fn();
const onSubmit = vi.fn();
const onReload = vi.fn();
const onRetryReferenceData = vi.fn();

function renderDialog(properties = {}) {
  return render(
    <EditProductDetailsDialog
      open
      product={product}
      isPending={false}
      error={null}
      referenceData={referenceData}
      isReferenceDataPending={false}
      referenceDataError={null}
      onClose={onClose}
      onSubmit={onSubmit}
      onReload={onReload}
      onRetryReferenceData={onRetryReferenceData}
      {...properties}
    />,
  );
}

function field(label) {
  return screen.getByLabelText(label);
}

async function choose(user, label, optionName) {
  await user.click(field(label));
  await user.click(await screen.findByRole("option", { name: optionName }));
}

async function save(user) {
  await user.click(screen.getByRole("button", { name: "Save changes" }));
}

describe("EditProductDetailsDialog", () => {
  beforeEach(() => {
    onClose.mockReset();
    onSubmit.mockReset();
    onReload.mockReset();
    onRetryReferenceData.mockReset();
    onSubmit.mockResolvedValue(undefined);
  });

  afterEach(cleanup);

  it("shows current values as business-friendly selections", () => {
    renderDialog();

    expect(field(/Product name/)).toHaveValue("Office Chair");
    expect(field("Description")).toHaveValue("Ergonomic office chair.");
    expect(field("Product category")).toHaveTextContent(
      "OFFICE — Office products",
    );
    expect(field(/Tax category/)).toHaveTextContent("GST15 — Standard GST");
  });

  it("requires a product name", async () => {
    const user = userEvent.setup();
    renderDialog();
    await user.clear(field(/Product name/));
    await save(user);

    expect(
      await screen.findByText("Product name is required."),
    ).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("validates text lengths", async () => {
    renderDialog();
    fireEvent.change(field(/Product name/), {
      target: { value: "N".repeat(101) },
    });
    fireEvent.change(field("Description"), {
      target: { value: "D".repeat(1001) },
    });
    fireEvent.click(screen.getByRole("button", { name: "Save changes" }));

    expect(
      await screen.findByText("Product name must not exceed 100 characters."),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Description must not exceed 1000 characters."),
    ).toBeInTheDocument();
  });

  it("clears the optional product category", async () => {
    const user = userEvent.setup();
    renderDialog();
    await choose(user, "Product category", "No product category");
    await save(user);

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit).toHaveBeenCalledWith({
      name: product.name,
      description: product.description,
      productCategoryId: null,
      taxCategoryId,
      rowVersion: product.rowVersion,
    });
  });

  it("submits edited values and selected reference IDs", async () => {
    const user = userEvent.setup();
    renderDialog();
    await user.clear(field(/Product name/));
    await user.type(field(/Product name/), "Updated Office Chair");
    await user.clear(field("Description"));
    await user.type(field("Description"), "Updated chair description.");
    await choose(user, "Product category", "FURN — Furniture");
    await choose(user, /Tax category/, "ZERO — Zero rated");
    await save(user);

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit).toHaveBeenCalledWith({
      name: "Updated Office Chair",
      description: "Updated chair description.",
      productCategoryId: otherProductCategoryId,
      taxCategoryId: otherTaxCategoryId,
      rowVersion: product.rowVersion,
    });
    expect(onClose).toHaveBeenCalledOnce();
  });

  it("sends an empty description as null", async () => {
    const user = userEvent.setup();
    renderDialog();
    await user.clear(field("Description"));
    await save(user);

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({ description: null }),
    );
  });

  it("keeps a currently assigned inactive category visible", async () => {
    const user = userEvent.setup();
    renderDialog({
      referenceData: { ...referenceData, productCategories: [] },
    });
    await user.click(field("Product category"));

    expect(
      screen.getByRole("option", { name: "Current selection is unavailable" }),
    ).toHaveAttribute("aria-disabled", "true");
  });

  it("keeps the dialog open when saving fails", async () => {
    const user = userEvent.setup();
    onSubmit.mockRejectedValue(
      new ApiError(409, "UPDATE_PRODUCT_CONFLICT", "Product changed."),
    );
    renderDialog();
    await save(user);

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onClose).not.toHaveBeenCalled();
  });

  it("shows a concurrency message and reload action", async () => {
    const user = userEvent.setup();
    renderDialog({
      error: new ApiError(409, "UPDATE_PRODUCT_CONFLICT", "Product changed."),
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "The product was modified by another user.",
    );
    await user.click(screen.getByRole("button", { name: "Reload product" }));
    expect(onReload).toHaveBeenCalledOnce();
  });

  it("shows reference-data loading and disables submission", () => {
    renderDialog({ isReferenceDataPending: true });

    expect(screen.getByText("Loading product options...")).toBeInTheDocument();
    expect(field(/Product name/)).toBeDisabled();
    expect(field("Product category")).toHaveAttribute("aria-disabled", "true");
    expect(screen.getByRole("button", { name: "Save changes" })).toBeDisabled();
  });

  it("shows a reference-data error and retries", async () => {
    const user = userEvent.setup();
    renderDialog({
      referenceDataError: new ApiError(
        500,
        "REFERENCE_ERROR",
        "Unable to retrieve product options.",
      ),
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Unable to retrieve product options.",
    );
    expect(screen.getByRole("button", { name: "Save changes" })).toBeDisabled();
    await user.click(screen.getByRole("button", { name: "Retry" }));
    expect(onRetryReferenceData).toHaveBeenCalledOnce();
  });

  it("disables all actions while saving", () => {
    renderDialog({ isPending: true });

    expect(field(/Product name/)).toBeDisabled();
    expect(field("Product category")).toHaveAttribute("aria-disabled", "true");
    expect(field(/Tax category/)).toHaveAttribute("aria-disabled", "true");
    expect(screen.getByRole("button", { name: "Cancel" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Saving..." })).toBeDisabled();
  });

  it("cancels without submitting", async () => {
    const user = userEvent.setup();
    renderDialog();
    await user.click(screen.getByRole("button", { name: "Cancel" }));
    expect(onClose).toHaveBeenCalledOnce();
    expect(onSubmit).not.toHaveBeenCalled();
  });
});
