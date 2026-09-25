import { useEffect, useState } from "react";
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Stack,
  TextField,
} from "@mui/material";
import { ApiError } from "../../../shared/api/ApiError";

export function CreatePurchaseOrderDialog({
  open,
  suppliers,
  products,
  isPending,
  error,
  onClose,
  onSubmit,
}) {
  const [supplierId, setSupplierId] = useState("");
  const [reference, setReference] = useState("");
  const [lines, setLines] = useState([
    { productId: "", quantity: "1", unitPrice: "0" },
  ]);
  const [validationError, setValidationError] = useState("");

  useEffect(() => {
    if (open) {
      setSupplierId("");
      setReference("");
      setLines([{ productId: "", quantity: "1", unitPrice: "0" }]);
      setValidationError("");
    }
  }, [open]);

  const submit = async (event) => {
    event.preventDefault();
    if (
      !supplierId ||
      !reference.trim() ||
      lines.some((line) => !line.productId)
    ) {
      setValidationError("Supplier, reference and a product are required.");
      return;
    }

    const requestLines = lines.map((line) => ({
      productId: line.productId,
      quantity: Number(line.quantity),
      unitPrice: Number(line.unitPrice),
    }));
    if (requestLines.some((line) => line.quantity <= 0 || line.unitPrice < 0)) {
      setValidationError(
        "Quantity must be greater than zero and unit price cannot be negative.",
      );
      return;
    }

    setValidationError("");
    try {
      await onSubmit({
        supplierId,
        reference: reference.trim(),
        lines: requestLines,
      });
      onClose();
    } catch {
      // The mutation exposes the API error below.
    }
  };

  return (
    <Dialog open={open} onClose={isPending ? undefined : onClose} fullWidth>
      <Stack component="form" onSubmit={submit}>
        <DialogTitle>Create purchase order</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {(validationError || error) && (
              <Alert severity="error">
                {validationError || getErrorMessage(error)}
              </Alert>
            )}
            <TextField
              select
              label="Supplier"
              value={supplierId}
              onChange={(event) => setSupplierId(event.target.value)}
              disabled={isPending}
            >
              {suppliers.map((supplier) => (
                <MenuItem key={supplier.supplierId} value={supplier.supplierId}>
                  {supplier.code} — {supplier.name}
                </MenuItem>
              ))}
            </TextField>
            <TextField
              label="Order reference"
              value={reference}
              onChange={(event) => setReference(event.target.value)}
              disabled={isPending}
            />
            {lines.map((line, index) => (
              <Stack
                key={index}
                direction={{ xs: "column", sm: "row" }}
                spacing={2}
              >
                <TextField
                  select
                  label="Product"
                  value={line.productId}
                  onChange={(event) =>
                    updateLine(index, "productId", event.target.value)
                  }
                  disabled={isPending}
                  sx={{ flex: 1 }}
                >
                  {products.map((product) => (
                    <MenuItem key={product.productId} value={product.productId}>
                      {product.sku} — {product.name}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField
                  label="Quantity"
                  type="number"
                  value={line.quantity}
                  onChange={(event) =>
                    updateLine(index, "quantity", event.target.value)
                  }
                  disabled={isPending}
                  slotProps={{ htmlInput: { min: 0.0001, step: 0.0001 } }}
                />
                <TextField
                  label="Unit price"
                  type="number"
                  value={line.unitPrice}
                  onChange={(event) =>
                    updateLine(index, "unitPrice", event.target.value)
                  }
                  disabled={isPending}
                  slotProps={{ htmlInput: { min: 0, step: 0.0001 } }}
                />
                {lines.length > 1 && (
                  <Button
                    color="error"
                    onClick={() => removeLine(index)}
                    disabled={isPending}
                  >
                    Remove
                  </Button>
                )}
              </Stack>
            ))}
            <Button
              onClick={() =>
                setLines((current) => [
                  ...current,
                  { productId: "", quantity: "1", unitPrice: "0" },
                ])
              }
              disabled={isPending}
            >
              Add line
            </Button>
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isPending}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isPending}>
            {isPending ? "Creating..." : "Create"}
          </Button>
        </DialogActions>
      </Stack>
    </Dialog>
  );

  function updateLine(index, field, value) {
    setLines((current) =>
      current.map((line, lineIndex) =>
        lineIndex === index ? { ...line, [field]: value } : line,
      ),
    );
  }

  function removeLine(index) {
    setLines((current) =>
      current.filter((_, lineIndex) => lineIndex !== index),
    );
  }
}

function getErrorMessage(error) {
  return error instanceof ApiError
    ? error.detail
    : "Unable to create purchase order.";
}
