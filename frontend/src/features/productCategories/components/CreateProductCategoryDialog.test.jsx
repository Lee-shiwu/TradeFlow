import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { CreateProductCategoryDialog } from "./CreateProductCategoryDialog";

const createdCategory = {
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  code: "OFFICE",
  name: "Office products",
  description: "Products used in the office.",
  status: "Active",
  createdAt: "2026-09-23T08:30:00+00:00",
  createdBy: "11111111-1111-1111-1111-111111111111",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

function renderDialog(overrides = {}) {
  const props = {
    open: true,
    isPending: false,
    error: null,
    onClose: vi.fn(),
    onSubmit: vi.fn().mockResolvedValue(createdCategory),
    ...overrides,
  };

  render(<CreateProductCategoryDialog {...props} />);
  return props;
}

describe("CreateProductCategoryDialog", () => {
  afterEach(cleanup);

  it("normalizes and submits valid values", async () => {
    const user = userEvent.setup();
    const props = renderDialog();

    await user.type(screen.getByLabelText(/Category code/), "office");
    await user.type(
      screen.getByLabelText(/Category name/),
      "  Office   products  ",
    );
    await user.type(
      screen.getByLabelText("Description"),
      "  Products used in the office.  ",
    );
    await user.click(screen.getByRole("button", { name: "Create category" }));

    await waitFor(() => {
      expect(props.onSubmit).toHaveBeenCalledWith({
        code: "OFFICE",
        name: "Office products",
        description: "Products used in the office.",
      });
    });
    expect(props.onClose).toHaveBeenCalledOnce();
  });

  it("shows validation errors without submitting", async () => {
    const user = userEvent.setup();
    const props = renderDialog();

    await user.click(screen.getByRole("button", { name: "Create category" }));

    expect(
      await screen.findByText("Product category code is required."),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Product category name is required."),
    ).toBeInTheDocument();
    expect(props.onSubmit).not.toHaveBeenCalled();
  });

  it("keeps the dialog open when submission fails", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(new Error("Create failed"));
    const props = renderDialog({ onSubmit });

    await user.type(screen.getByLabelText(/Category code/), "OFFICE");
    await user.type(screen.getByLabelText(/Category name/), "Office products");
    await user.click(screen.getByRole("button", { name: "Create category" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(props.onClose).not.toHaveBeenCalled();
    expect(
      screen.getByRole("dialog", { name: "Create product category" }),
    ).toBeInTheDocument();
  });

  it("shows the API error", () => {
    renderDialog({
      error: new ApiError(
        409,
        "PRODUCT_CATEGORY_CODE_ALREADY_EXISTS",
        "A product category with this code already exists in the organisation.",
      ),
    });

    expect(
      screen.getByText(
        "A product category with this code already exists in the organisation.",
      ),
    ).toBeInTheDocument();
  });

  it("disables actions while creating", () => {
    renderDialog({ isPending: true });

    expect(screen.getByRole("button", { name: "Creating..." })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Cancel" })).toBeDisabled();
  });
});
