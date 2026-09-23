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
import { EditProductCategoryDetailsDialog } from "./EditProductCategoryDetailsDialog";

const category = {
  productCategoryId: "77777777-7777-7777-7777-777777777777",
  code: "OFFICE",
  name: "Office products",
  description: "Products used in the office.",
  status: "Active",
  createdAt: "2026-09-20T10:00:00+00:00",
  createdBy: "11111111-1111-1111-1111-111111111111",
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: "AAAAAAAAB9E=",
};

const onClose = vi.fn();
const onSubmit = vi.fn();
const onReload = vi.fn();

function renderDialog(properties = {}) {
  return render(
    <EditProductCategoryDetailsDialog
      open
      category={category}
      isPending={false}
      error={null}
      onClose={onClose}
      onSubmit={onSubmit}
      onReload={onReload}
      {...properties}
    />,
  );
}

describe("EditProductCategoryDetailsDialog", () => {
  beforeEach(() => {
    onClose.mockReset();
    onSubmit.mockReset();
    onReload.mockReset();
    onSubmit.mockResolvedValue(undefined);
  });

  afterEach(cleanup);

  it("shows the immutable code and current editable values", () => {
    renderDialog();

    expect(screen.getByLabelText("Category code")).toHaveValue("OFFICE");
    expect(screen.getByLabelText(/Category name/)).toHaveValue(
      "Office products",
    );
    expect(screen.getByLabelText("Description")).toHaveValue(
      "Products used in the office.",
    );
    expect(screen.getByLabelText("Category code")).toBeDisabled();
  });

  it("validates required name and text lengths", async () => {
    renderDialog();
    fireEvent.change(screen.getByLabelText(/Category name/), {
      target: { value: "" },
    });
    fireEvent.change(screen.getByLabelText("Description"), {
      target: { value: "D".repeat(1001) },
    });
    fireEvent.click(screen.getByRole("button", { name: "Save changes" }));

    expect(
      await screen.findByText("Product category name is required."),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        "Product category description must not exceed 1000 characters.",
      ),
    ).toBeInTheDocument();
    expect(onSubmit).not.toHaveBeenCalled();
  });

  it("submits normalized values with the current row version", async () => {
    const user = userEvent.setup();
    renderDialog();
    await user.clear(screen.getByLabelText(/Category name/));
    await user.type(screen.getByLabelText(/Category name/), "Updated category");
    await user.clear(screen.getByLabelText("Description"));
    await user.click(screen.getByRole("button", { name: "Save changes" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onSubmit).toHaveBeenCalledWith({
      name: "Updated category",
      description: null,
      rowVersion: category.rowVersion,
    });
    expect(onClose).toHaveBeenCalledOnce();
  });

  it("keeps the dialog open when saving fails", async () => {
    const user = userEvent.setup();
    onSubmit.mockRejectedValue(new Error("Update failed"));
    renderDialog();
    await user.click(screen.getByRole("button", { name: "Save changes" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(onClose).not.toHaveBeenCalled();
  });

  it("shows concurrency guidance and reloads", async () => {
    const user = userEvent.setup();
    renderDialog({
      error: new ApiError(
        409,
        "UPDATE_PRODUCT_CATEGORY_CONCURRENCY_CONFLICT",
        "Category changed.",
      ),
    });

    expect(screen.getByRole("alert")).toHaveTextContent(
      "The product category was modified by another user.",
    );
    await user.click(screen.getByRole("button", { name: "Reload category" }));
    expect(onReload).toHaveBeenCalledOnce();
  });

  it("disables editing and closing while saving", () => {
    renderDialog({ isPending: true });

    expect(screen.getByLabelText(/Category name/)).toBeDisabled();
    expect(screen.getByLabelText("Description")).toBeDisabled();
    expect(screen.getByRole("button", { name: "Cancel" })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Saving..." })).toBeDisabled();
  });
});
