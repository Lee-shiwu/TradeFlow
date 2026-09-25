import { cleanup, render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";
import { ApiError } from "../../../shared/api/ApiError";
import { CreateTaxCategoryDialog } from "./CreateTaxCategoryDialog";

const createdCategory = {
  taxCategoryId: "tax-category-id",
  code: "REGIONAL",
  name: "Regional levy",
  description: "Regional standard tax.",
  rate: 0.125,
  treatment: "StandardRated",
  status: "Active",
  effectiveFrom: "2026-10-01",
  effectiveTo: null,
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
  render(<CreateTaxCategoryDialog {...props} />);
  return props;
}

describe("CreateTaxCategoryDialog", () => {
  afterEach(cleanup);

  it("normalizes values, converts percent, and submits", async () => {
    const user = userEvent.setup();
    const props = renderDialog();

    await user.type(screen.getByLabelText(/Tax category code/), "regional");
    await user.type(
      screen.getByLabelText(/Tax category name/),
      "  Regional   levy  ",
    );
    await user.type(
      screen.getByLabelText("Description"),
      "  Regional standard tax.  ",
    );
    await user.type(screen.getByLabelText(/Rate/), "12.5");
    await user.type(screen.getByLabelText(/Effective from/), "2026-10-01");
    await user.click(
      screen.getByRole("button", { name: "Create tax category" }),
    );

    await waitFor(() => {
      expect(props.onSubmit).toHaveBeenCalledWith({
        code: "REGIONAL",
        name: "Regional levy",
        description: "Regional standard tax.",
        rate: 0.125,
        treatment: "StandardRated",
        effectiveFrom: "2026-10-01",
        effectiveTo: null,
      });
    });
    expect(props.onClose).toHaveBeenCalledOnce();
  });

  it("shows required-field errors without submitting", async () => {
    const user = userEvent.setup();
    const props = renderDialog();

    await user.click(
      screen.getByRole("button", { name: "Create tax category" }),
    );

    expect(
      await screen.findByText("Tax category code is required."),
    ).toBeInTheDocument();
    expect(
      screen.getByText("Tax category name is required."),
    ).toBeInTheDocument();
    expect(screen.getByText("Tax rate is required.")).toBeInTheDocument();
    expect(
      screen.getByText("Effective-from date is required."),
    ).toBeInTheDocument();
    expect(props.onSubmit).not.toHaveBeenCalled();
  });

  it("validates treatment-rate and effective-date rules", async () => {
    const user = userEvent.setup();
    const props = renderDialog();

    await user.type(screen.getByLabelText(/Tax category code/), "regional");
    await user.type(screen.getByLabelText(/Tax category name/), "Regional");
    await user.type(screen.getByLabelText(/Rate/), "0");
    await user.type(screen.getByLabelText(/Effective from/), "2026-10-01");
    await user.type(screen.getByLabelText(/Effective to/), "2026-09-30");
    await user.click(
      screen.getByRole("button", { name: "Create tax category" }),
    );

    expect(
      await screen.findByText(
        "A standard-rated category must have a rate above 0%.",
      ),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        "Effective-to date cannot be earlier than effective-from date.",
      ),
    ).toBeInTheDocument();
    expect(props.onSubmit).not.toHaveBeenCalled();
  });

  it("keeps the dialog open and shows an API error", async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn().mockRejectedValue(new Error("Create failed"));
    const props = renderDialog({
      onSubmit,
      error: new ApiError(
        409,
        "TAX_CATEGORY_CODE_ALREADY_EXISTS",
        "A tax category with this code already exists.",
      ),
    });

    expect(
      screen.getByText("A tax category with this code already exists."),
    ).toBeInTheDocument();
    await user.type(screen.getByLabelText(/Tax category code/), "regional");
    await user.type(screen.getByLabelText(/Tax category name/), "Regional");
    await user.type(screen.getByLabelText(/Rate/), "12.5");
    await user.type(screen.getByLabelText(/Effective from/), "2026-10-01");
    await user.click(
      screen.getByRole("button", { name: "Create tax category" }),
    );

    await waitFor(() => expect(onSubmit).toHaveBeenCalledOnce());
    expect(props.onClose).not.toHaveBeenCalled();
    expect(
      screen.getByRole("dialog", { name: "Create tax category" }),
    ).toBeInTheDocument();
  });

  it("disables actions while creating", () => {
    renderDialog({ isPending: true });

    expect(screen.getByRole("button", { name: "Creating..." })).toBeDisabled();
    expect(screen.getByRole("button", { name: "Cancel" })).toBeDisabled();
  });
});
