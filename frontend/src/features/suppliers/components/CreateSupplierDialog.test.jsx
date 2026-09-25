import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { CreateSupplierDialog } from "./CreateSupplierDialog";

function renderDialog(overrides = {}) {
  const props = {
    open: true,
    isPending: false,
    error: null,
    onClose: vi.fn(),
    onSubmit: vi.fn().mockResolvedValue({ supplierId: "supplier-id" }),
    ...overrides,
  };
  render(<CreateSupplierDialog {...props} />);
  return props;
}

describe("CreateSupplierDialog", () => {
  afterEach(cleanup);

  it("normalizes and submits a supplier", async () => {
    const user = userEvent.setup();
    const props = renderDialog();

    await user.type(screen.getByLabelText(/Supplier code/), "office-01");
    await user.type(
      screen.getByLabelText(/Supplier name/),
      "  Auckland   Office Supplies  ",
    );
    await user.click(screen.getByRole("button", { name: "Create supplier" }));

    await waitFor(() => {
      expect(props.onSubmit).toHaveBeenCalledWith({
        code: "OFFICE-01",
        name: "Auckland Office Supplies",
      });
    });
    expect(props.onClose).toHaveBeenCalledOnce();
  });

  it("validates required fields", async () => {
    const user = userEvent.setup();
    const props = renderDialog();

    await user.click(screen.getByRole("button", { name: "Create supplier" }));

    expect(
      await screen.findByText("Supplier code is required."),
    ).toBeInTheDocument();
    expect(screen.getByText("Supplier name is required.")).toBeInTheDocument();
    expect(props.onSubmit).not.toHaveBeenCalled();
  });

  it("keeps the dialog open after an API failure", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(new Error("Create failed"));
    const props = renderDialog({
      onSubmit,
      error: new ApiError(
        409,
        "SUPPLIER_CODE_ALREADY_EXISTS",
        "A supplier with this code already exists in the organisation.",
      ),
    });

    expect(
      screen.getByText(
        "A supplier with this code already exists in the organisation.",
      ),
    ).toBeInTheDocument();
    await user.type(screen.getByLabelText(/Supplier code/), "office");
    await user.type(screen.getByLabelText(/Supplier name/), "Office supplier");
    await user.click(screen.getByRole("button", { name: "Create supplier" }));

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(props.onClose).not.toHaveBeenCalled();
  });

  it("disables actions while creating", () => {
    renderDialog({ isPending: true });

    expect(screen.getByRole("button", { name: "Creating..." })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Cancel" })).toBeDisabled();
  });
});
