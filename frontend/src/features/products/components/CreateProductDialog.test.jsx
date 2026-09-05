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
import { CreateProductDialog } from "./CreateProductDialog";

const unitOfMeasureId = "44444444-4444-4444-4444-444444444444";
const productCategoryId = "77777777-7777-7777-7777-777777777777";
const taxCategoryId = "55555555-5555-5555-5555-555555555555";

const referenceData = {
  unitsOfMeasure: [{ id: unitOfMeasureId, code: "EA", name: "Each" }],
  productCategories: [
    { id: productCategoryId, code: "OFFICE", name: "Office products" },
  ],
  taxCategories: [{ id: taxCategoryId, code: "GST15", name: "Standard GST" }],
};

const onClose = vi.fn();
const onSubmit = vi.fn();
const onRetryReferenceData = vi.fn();

function createDialog(properties = {}) {
  return (
    <CreateProductDialog
      open
      isPending={false}
      error={null}
      referenceData={referenceData}
      isReferenceDataPending={false}
      referenceDataError={null}
      onClose={onClose}
      onSubmit={onSubmit}
      onRetryReferenceData={onRetryReferenceData}
      {...properties}
    />
  );
}

function renderDialog(properties = {}) {
  return render(createDialog(properties));
}

function field(label) {
  return screen.getByLabelText(label);
}

async function choose(user, label, optionName) {
  await user.click(field(label));
  await user.click(await screen.findByRole("option", { name: optionName }));
}

async function enterValidProduct(user, includeOptionalFields = true) {
  await user.type(field(/Product SKU/), "chair-001");
  await user.type(field(/Product name/), "Office Chair");
  if (includeOptionalFields) {
    await user.type(field("Description"), "Ergonomic office chair.");
  }
  await choose(user, /Unit of measure/, "EA — Each");
  if (includeOptionalFields) {
    await choose(user, "Product category", "OFFICE — Office products");
  }
  await choose(user, /Tax category/, "GST15 — Standard GST");
}

async function submit(user) {
  await user.click(screen.getByRole("button", { name: "Create product" }));
}

describe("CreateProductDialog", () => {
  beforeEach(() => {
    onClose.mockReset();
    onSubmit.mockReset();
    onRetryReferenceData.mockReset();
    onSubmit.mockResolvedValue({ productId: "product-id", sku: "CHAIR-001" });
  });

  afterEach(cleanup);

  it("shows business-friendly reference-data choices", async () => {
    const user = userEvent.setup();
    renderDialog();

    await user.click(field(/Unit of measure/));
    expect(
      screen.getByRole("option", { name: "EA — Each" }),
    ).toBeInTheDocument();
    await user.keyboard("{Escape}");
    await user.click(field("Product category"));
    expect(
      screen.getByRole("option", { name: "No product category" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("option", { name: "OFFICE — Office products" }),
    ).toBeInTheDocument();
    await user.keyboard("{Escape}");
    await user.click(field(/Tax category/));
    expect(
      screen.getByRole("option", { name: "GST15 — Standard GST" }),
    ).toBeInTheDocument();
  });

  it("requires mandatory fields", async () => {
    const user = userEvent.setup();
    renderDialog();
    await submit(user);

    expect(
      await screen.findByText("Product SKU is required."),
    ).toBeInTheDocument();
    expect(screen.getByText("Product name is required.")).toBeInTheDocument();
    expect(
      screen.getByText("Unit of measure is required."),
    ).toBeInTheDocument();
    expect(screen.getByText("Tax category is required.")).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("validates text formats and lengths", async () => {
    renderDialog();
    fireEvent.change(field(/Product SKU/), {
      target: { value: "invalid sku" },
    });
    fireEvent.change(field(/Product name/), {
      target: { value: "N".repeat(101) },
    });
    fireEvent.change(field("Description"), {
      target: { value: "D".repeat(1001) },
    });
    fireEvent.click(screen.getByRole("button", { name: "Create product" }));

    expect(
      await screen.findByText(
        "Product SKU can only contain letters, numbers, and hyphens.",
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Product name must not exceed 100 characters."),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Description must not exceed 1000 characters."),
    ).toBeInTheDocument();
  });

  it("submits selected reference IDs and closes", async () => {
    const user = userEvent.setup();
    renderDialog();
    await enterValidProduct(user);
    await submit(user);

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit).toHaveBeenCalledWith({
      sku: "CHAIR-001",
      name: "Office Chair",
      description: "Ergonomic office chair.",
      unitOfMeasureId,
      productCategoryId,
      taxCategoryId,
    });
    expect(onClose).toHaveBeenCalledOnce();
  });

  it("submits optional fields as null", async () => {
    const user = userEvent.setup();
    renderDialog();
    await enterValidProduct(user, false);
    await submit(user);

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit).toHaveBeenCalledWith(
      expect.objectContaining({
        description: null,
        productCategoryId: null,
      }),
    );
  });

  it("keeps the dialog open when creation fails", async () => {
    const user = userEvent.setup();
    onSubmit.mockRejectedValue(
      new ApiError(409, "PRODUCT_EXISTS", "SKU already exists."),
    );
    renderDialog();
    await enterValidProduct(user);
    await submit(user);

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onClose).not.toHaveBeenCalled();
  });

  it("shows reference-data loading and disables submission", () => {
    renderDialog({ isReferenceDataPending: true });

    expect(screen.getByText("Loading product options...")).toBeInTheDocument();
    expect(field(/Product SKU/)).toBeDisabled();
    expect(field(/Unit of measure/)).toHaveAttribute("aria-disabled", "true");
    expect(
      screen.getByRole("button", { name: "Create product" }),
    ).toBeDisabled();
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
    expect(
      screen.getByRole("button", { name: "Create product" }),
    ).toBeDisabled();
    await user.click(screen.getByRole("button", { name: "Retry" }));
    expect(onRetryReferenceData).toHaveBeenCalledOnce();
  });

  it("shows creation errors", () => {
    renderDialog({
      error: new ApiError(409, "PRODUCT_EXISTS", "SKU already exists."),
    });
    expect(screen.getByRole("alert")).toHaveTextContent("SKU already exists.");
  });

  it("cancels without submitting", async () => {
    const user = userEvent.setup();
    renderDialog();
    await user.click(screen.getByRole("button", { name: "Cancel" }));
    expect(onClose).toHaveBeenCalledOnce();
    expect(onSubmit).not.toHaveBeenCalled();
  });
});
