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

const response = {
  productId: "33333333-3333-3333-3333-333333333333",
  sku: "CHAIR-001",
};

const onCloseMock = vi.fn();
const onSubmitMock = vi.fn();

function createDialog(properties = {}) {
  return (
    <CreateProductDialog
      open
      isPending={false}
      error={null}
      onClose={onCloseMock}
      onSubmit={onSubmitMock}
      {...properties}
    />
  );
}

function renderDialog(properties = {}) {
  return render(createDialog(properties));
}

function getSkuInput() {
  return screen.getByLabelText(/Product SKU/);
}

function getNameInput() {
  return screen.getByLabelText(/Product name/);
}

function getDescriptionInput() {
  return screen.getByLabelText("Description");
}

function getUnitOfMeasureInput() {
  return screen.getByLabelText(/Unit of measure ID/);
}

function getProductCategoryInput() {
  return screen.getByLabelText("Product category ID");
}

function getTaxCategoryInput() {
  return screen.getByLabelText(/Tax category ID/);
}

async function enterValidProduct(user) {
  await user.type(getSkuInput(), "chair-001");
  await user.type(getNameInput(), "Office Chair");
  await user.type(getDescriptionInput(), "Ergonomic office chair.");
  await user.type(getUnitOfMeasureInput(), unitOfMeasureId);
  await user.type(getProductCategoryInput(), productCategoryId);
  await user.type(getTaxCategoryInput(), taxCategoryId);
}

async function submitForm(user) {
  await user.click(
    screen.getByRole("button", {
      name: "Create product",
    }),
  );
}

describe("CreateProductDialog", () => {
  beforeEach(() => {
    onCloseMock.mockReset();
    onSubmitMock.mockReset();

    onSubmitMock.mockResolvedValue(response);
  });

  afterEach(() => {
    cleanup();
  });

  it("shows empty creation fields", () => {
    renderDialog();

    expect(getSkuInput()).toHaveValue("");
    expect(getNameInput()).toHaveValue("");
    expect(getDescriptionInput()).toHaveValue("");
    expect(getUnitOfMeasureInput()).toHaveValue("");
    expect(getProductCategoryInput()).toHaveValue("");
    expect(getTaxCategoryInput()).toHaveValue("");
  });

  it("requires a product SKU", async () => {
    const user = userEvent.setup();

    renderDialog();

    await submitForm(user);

    expect(
      await screen.findByText("Product SKU is required."),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("limits the SKU to 50 characters", async () => {
    renderDialog();

    fireEvent.change(getSkuInput(), {
      target: {
        value: "A".repeat(51),
      },
    });

    fireEvent.click(
      screen.getByRole("button", {
        name: "Create product",
      }),
    );

    expect(
      await screen.findByText("Product SKU must not exceed 50 characters."),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("rejects invalid SKU characters", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.type(getSkuInput(), "chair 001");

    await submitForm(user);

    expect(
      await screen.findByText(
        "Product SKU can only contain letters, numbers, and hyphens.",
      ),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("requires a product name", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.type(getSkuInput(), "CHAIR-001");

    await submitForm(user);

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
        name: "Create product",
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
        name: "Create product",
      }),
    );

    expect(
      await screen.findByText("Description must not exceed 1000 characters."),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("requires a unit of measure ID", async () => {
    const user = userEvent.setup();

    renderDialog();

    await submitForm(user);

    expect(
      await screen.findByText("Unit of measure ID is required."),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("rejects an invalid unit of measure ID", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.type(getUnitOfMeasureInput(), "invalid-unit-id");

    await submitForm(user);

    expect(
      await screen.findByText(
        "Unit of measure ID must be a valid non-empty GUID.",
      ),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("rejects an invalid product category ID", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.type(getProductCategoryInput(), "invalid-category-id");

    await submitForm(user);

    expect(
      await screen.findByText(
        "Product category ID must be a valid non-empty GUID.",
      ),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("requires a tax category ID", async () => {
    const user = userEvent.setup();

    renderDialog();

    await submitForm(user);

    expect(
      await screen.findByText("Tax category ID is required."),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("rejects an invalid tax category ID", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.type(getTaxCategoryInput(), "invalid-tax-id");

    await submitForm(user);

    expect(
      await screen.findByText(
        "Tax category ID must be a valid non-empty GUID.",
      ),
    ).toBeInTheDocument();

    expect(onSubmitMock).not.toHaveBeenCalled();
  });

  it("submits a valid product", async () => {
    const user = userEvent.setup();

    renderDialog();

    await enterValidProduct(user);
    await submitForm(user);

    await waitFor(() => {
      expect(onSubmitMock).toHaveBeenCalledOnce();
    });

    expect(onSubmitMock).toHaveBeenCalledWith({
      sku: "CHAIR-001",
      name: "Office Chair",
      description: "Ergonomic office chair.",
      unitOfMeasureId,
      productCategoryId,
      taxCategoryId,
    });
  });

  it("submits optional fields as null", async () => {
    const user = userEvent.setup();

    renderDialog();

    await user.type(getSkuInput(), "chair-001");
    await user.type(getNameInput(), "Office Chair");
    await user.type(getUnitOfMeasureInput(), unitOfMeasureId);
    await user.type(getTaxCategoryInput(), taxCategoryId);

    await submitForm(user);

    await waitFor(() => {
      expect(onSubmitMock).toHaveBeenCalledOnce();
    });

    expect(onSubmitMock).toHaveBeenCalledWith({
      sku: "CHAIR-001",
      name: "Office Chair",
      description: null,
      unitOfMeasureId,
      productCategoryId: null,
      taxCategoryId,
    });
  });

  it("closes after creation succeeds", async () => {
    const user = userEvent.setup();

    renderDialog();

    await enterValidProduct(user);
    await submitForm(user);

    await waitFor(() => {
      expect(onCloseMock).toHaveBeenCalledOnce();
    });
  });

  it("does not close when creation fails", async () => {
    const user = userEvent.setup();

    onSubmitMock.mockRejectedValue(
      new ApiError(
        409,
        "PRODUCT_SKU_ALREADY_EXISTS",
        "A product with this SKU already exists in the organisation.",
      ),
    );

    renderDialog();

    await enterValidProduct(user);
    await submitForm(user);

    await waitFor(() => {
      expect(onSubmitMock).toHaveBeenCalledOnce();
    });

    expect(onCloseMock).not.toHaveBeenCalled();
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

  it("shows an API error", () => {
    renderDialog({
      error: new ApiError(
        409,
        "PRODUCT_SKU_ALREADY_EXISTS",
        "A product with this SKU already exists in the organisation.",
        "conflict-trace-id",
      ),
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "A product with this SKU already exists in the organisation.",
    );
  });

  it("shows a fallback message for an unknown error", () => {
    renderDialog({
      error: new Error("Unknown failure"),
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "Unable to create the product.",
    );
  });

  it("disables the form while creating", () => {
    renderDialog({
      isPending: true,
    });

    expect(getSkuInput()).toBeDisabled();
    expect(getNameInput()).toBeDisabled();
    expect(getDescriptionInput()).toBeDisabled();
    expect(getUnitOfMeasureInput()).toBeDisabled();
    expect(getProductCategoryInput()).toBeDisabled();
    expect(getTaxCategoryInput()).toBeDisabled();

    expect(
      screen.getByRole("button", {
        name: "Cancel",
      }),
    ).toBeDisabled();

    expect(
      screen.getByRole("button", {
        name: "Creating...",
      }),
    ).toBeDisabled();
  });

  it("clears entered values when reopened", async () => {
    const user = userEvent.setup();

    const { rerender } = renderDialog();

    await user.type(getSkuInput(), "CHAIR-001");
    await user.type(getNameInput(), "Office Chair");

    rerender(
      createDialog({
        open: false,
      }),
    );

    rerender(
      createDialog({
        open: true,
      }),
    );

    await waitFor(() => {
      expect(getSkuInput()).toHaveValue("");
      expect(getNameInput()).toHaveValue("");
    });
  });
});
