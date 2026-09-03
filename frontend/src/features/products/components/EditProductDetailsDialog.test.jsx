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

const product = {
  productId: "33333333-3333-3333-3333-333333333333",
  sku: "CHAIR-001",
  name: "Office Chair",
  description: "Ergonomic office chair.",
  unitOfMeasureId: "44444444-4444-4444-4444-444444444444",
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  taxCategoryId: "55555555-5555-5555-5555-555555555555",
  status: "Active",
  createdAt: "2026-08-27T00:00:00+00:00",
  createdBy: "66666666-6666-6666-6666-666666666666",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

const onCloseMock = vi.fn();
const onSubmitMock = vi.fn();
const onReloadMock = vi.fn();

function renderDialog(properties = {}) {
  return render(
    <EditProductDetailsDialog
      open
      product={product}
      isPending={false}
      error={null}
      onClose={onCloseMock}
      onSubmit={onSubmitMock}
      onReload={onReloadMock}
      {...properties}
    />,
  );
}

function getNameInput() {
  return screen.getByLabelText(/Product name/);
}

function getDescriptionInput() {
  return screen.getByLabelText("Description");
}

function getProductCategoryInput() {
  return screen.getByLabelText("Product category ID");
}

function getTaxCategoryInput() {
  return screen.getByLabelText(/Tax category ID/);
}

describe("EditProductDetailsDialog", () => {
  beforeEach(() => {
    onCloseMock.mockReset();
    onSubmitMock.mockReset();
    onReloadMock.mockReset();

    onSubmitMock.mockResolvedValue(undefined);
  });

  afterEach(() => {
    cleanup();
  });

  it("shows the current product values", () => {
    renderDialog();

    expect(getNameInput()).toHaveValue("Office Chair");

    expect(getDescriptionInput()).toHaveValue("Ergonomic office chair.");

    expect(getProductCategoryInput()).toHaveValue(product.productCategoryId);

    expect(getTaxCategoryInput()).toHaveValue(product.taxCategoryId);
  });

  it("requires a product name", async () => {
    const user = userEvent.setup();

    renderDialog();

    const nameInput = getNameInput();

    await user.clear(nameInput);

    await user.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    expect(
      await screen.findByText("Product name is required."),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("limits the product name to 100 characters", async () => {
    renderDialog();

    fireEvent.change(getNameInput(), {
      target: {
        value: "A".repeat(101),
      },
    });

    fireEvent.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    expect(
      await screen.findByText("Product name must not exceed 100 characters."),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("limits the description to 1000 characters", async () => {
    renderDialog();

    fireEvent.change(getDescriptionInput(), {
      target: {
        value: "A".repeat(1001),
      },
    });

    fireEvent.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    expect(
      await screen.findByText("Description must not exceed 1000 characters."),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("rejects an invalid product category ID", async () => {
    const user = userEvent.setup();

    renderDialog();

    const productCategoryInput = getProductCategoryInput();

    await user.clear(productCategoryInput);

    await user.type(productCategoryInput, "invalid-category-id");

    await user.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    expect(
      await screen.findByText(
        "Product category ID must be a valid non-empty GUID.",
      ),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("requires a valid tax category ID", async () => {
    const user = userEvent.setup();

    renderDialog();

    const taxCategoryInput = getTaxCategoryInput();

    await user.clear(taxCategoryInput);

    await user.type(taxCategoryInput, "invalid-tax-category-id");

    await user.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    expect(
      await screen.findByText(
        "Tax category ID must be a valid non-empty GUID.",
      ),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("allows the optional product category to be cleared", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.clear(getProductCategoryInput());

    await user.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    await waitFor(() => {
      expect(onSubmitMock).toHaveBeenCalledOnce();
    });

    expect(onSubmitMock).toHaveBeenCalledWith({
      name: product.name,
      description: product.description,
      productCategoryId: null,
      taxCategoryId: product.taxCategoryId,
      rowVersion: product.rowVersion,
    });
  });

  it("submits the edited values and rowVersion", async () => {
    const user = userEvent.setup();

    renderDialog();

    const nameInput = getNameInput();

    const descriptionInput = getDescriptionInput();

    await user.clear(nameInput);
    await user.type(nameInput, "Updated Office Chair");

    await user.clear(descriptionInput);
    await user.type(descriptionInput, "Updated chair description.");

    await user.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    await waitFor(() => {
      expect(onSubmitMock).toHaveBeenCalledOnce();
    });

    expect(onSubmitMock).toHaveBeenCalledWith({
      name: "Updated Office Chair",
      description: "Updated chair description.",
      productCategoryId: product.productCategoryId,
      taxCategoryId: product.taxCategoryId,
      rowVersion: product.rowVersion,
    });
  });

  it("sends an empty description as null", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.clear(getDescriptionInput());

    await user.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    await waitFor(() => {
      expect(onSubmitMock).toHaveBeenCalledOnce();
    });

    expect(onSubmitMock).toHaveBeenCalledWith(
      expect.objectContaining({
        description: null,
      }),
    );
  });

  it("closes without submitting when cancelled", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.click(
      screen.getByRole("button", {
        name: "Cancel",
      }),
    );

    expect(onCloseMock).toHaveBeenCalledOnce();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("closes after the update succeeds", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    await waitFor(() => {
      expect(onCloseMock).toHaveBeenCalledOnce();
    });
  });

  it("does not close when the update fails", async () => {
    const user = userEvent.setup();

    onSubmitMock.mockRejectedValue(
      new ApiError(
        409,
        "UPDATE_PRODUCT_CONCURRENCY_CONFLICT",
        "The product was modified by another user.",
      ),
    );

    renderDialog();

    await user.click(
      screen.getByRole("button", {
        name: "Save changes",
      }),
    );

    await waitFor(() => {
      expect(onSubmitMock).toHaveBeenCalledOnce();
    });

    expect(onCloseMock).not.toHaveBeenCalled();
  });

  it("disables the form while saving", () => {
    renderDialog({
      isPending: true,
    });

    expect(getNameInput()).toBeDisabled();

    expect(getDescriptionInput()).toBeDisabled();

    expect(getProductCategoryInput()).toBeDisabled();

    expect(getTaxCategoryInput()).toBeDisabled();

    expect(
      screen.getByRole("button", {
        name: "Cancel",
      }),
    ).toBeDisabled();

    expect(
      screen.getByRole("button", {
        name: "Saving...",
      }),
    ).toBeDisabled();
  });

  it("shows an API error", () => {
    renderDialog({
      error: new ApiError(
        400,
        "UPDATE_PRODUCT_NAME_INVALID",
        "The product name is invalid.",
        "validation-trace-id",
      ),
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "The product name is invalid.",
    );
  });

  it("shows a concurrency error and reloads the product", async () => {
    const user = userEvent.setup();

    renderDialog({
      error: new ApiError(
        409,
        "UPDATE_PRODUCT_CONCURRENCY_CONFLICT",
        "The product was modified by another user.",
        "concurrency-trace-id",
      ),
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "The product was modified by another user.",
    );

    await user.click(
      screen.getByRole("button", {
        name: "Reload product",
      }),
    );

    expect(onReloadMock).toHaveBeenCalledOnce();
  });

  it("does not show reload for a normal API error", () => {
    renderDialog({
      error: new ApiError(
        500,
        "UNEXPECTED_ERROR",
        "Unable to contact the product service.",
      ),
    });

    expect(
      screen.queryByRole("button", {
        name: "Reload product",
      }),
    ).not.toBeInTheDocument();
  });
});
