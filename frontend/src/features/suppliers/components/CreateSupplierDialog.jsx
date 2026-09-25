/** @typedef {import("../api/supplierTypes.js").CreateSupplierRequest} CreateSupplierRequest */
/** @typedef {import("../api/supplierTypes.js").CreateSupplierResponse} CreateSupplierResponse */

import { zodResolver } from "@hookform/resolvers/zod";
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
  TextField,
} from "@mui/material";
import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { ApiError } from "../../../shared/api/ApiError";

const codePattern = /^[A-Z0-9-]+$/;
const createSupplierSchema = z.object({
  code: z
    .string()
    .trim()
    .min(1, "Supplier code is required.")
    .max(50, "Supplier code must not exceed 50 characters.")
    .transform((value) => value.toUpperCase())
    .refine(
      (value) => codePattern.test(value),
      "Supplier code can only contain letters, numbers, and hyphens.",
    ),
  name: z
    .string()
    .trim()
    .min(1, "Supplier name is required.")
    .max(150, "Supplier name must not exceed 150 characters.")
    .transform((value) => value.replace(/\s+/g, " ")),
});

const emptyFormValues = { code: "", name: "" };

/**
 * @param {{
 *   open: boolean,
 *   isPending: boolean,
 *   error: unknown,
 *   onClose: () => void,
 *   onSubmit: (request: CreateSupplierRequest) => Promise<CreateSupplierResponse>
 * }} props
 */
export function CreateSupplierDialog({
  open,
  isPending,
  error,
  onClose,
  onSubmit,
}) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(createSupplierSchema),
    defaultValues: emptyFormValues,
  });

  useEffect(() => {
    if (open) {
      reset(emptyFormValues);
    }
  }, [open, reset]);

  async function handleValidSubmit(values) {
    try {
      const createdSupplier = await onSubmit(values);
      onClose();
      return createdSupplier;
    } catch {
      return undefined;
    }
  }

  function handleDialogClose() {
    if (!isPending) {
      onClose();
    }
  }

  return (
    <Dialog
      open={open}
      onClose={handleDialogClose}
      fullWidth
      maxWidth="sm"
      aria-labelledby="create-supplier-dialog-title"
    >
      <Box
        component="form"
        noValidate
        onSubmit={handleSubmit(handleValidSubmit)}
      >
        <DialogTitle id="create-supplier-dialog-title">
          Create supplier
        </DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Supplier code"
              required
              autoFocus
              disabled={isPending}
              error={Boolean(errors.code)}
              helperText={errors.code?.message}
              {...register("code")}
            />
            <TextField
              label="Supplier name"
              required
              disabled={isPending}
              error={Boolean(errors.name)}
              helperText={errors.name?.message}
              {...register("name")}
            />
            {error && <Alert severity="error">{getErrorMessage(error)}</Alert>}
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button
            type="button"
            disabled={isPending}
            onClick={handleDialogClose}
          >
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isPending}>
            {isPending ? "Creating..." : "Create supplier"}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}

function getErrorMessage(error) {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to create the supplier.";
}
