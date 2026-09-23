/**
 * @typedef {import("../api/productCategoryDetailsTypes.js").ProductCategoryDetails}
 * ProductCategoryDetails
 */

/**
 * @typedef {import("../api/updateProductCategoryDetailsTypes.js").UpdateProductCategoryDetailsRequest}
 * UpdateProductCategoryDetailsRequest
 */

/**
 * @typedef {Object} EditProductCategoryDetailsDialogProps
 * @property {boolean} open
 * @property {ProductCategoryDetails} category
 * @property {boolean} isPending
 * @property {unknown} error
 * @property {() => void} onClose
 * @property {(request: UpdateProductCategoryDetailsRequest) => Promise<unknown>} onSubmit
 * @property {() => void} onReload
 */

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

const editProductCategoryDetailsSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Product category name is required.")
    .max(100, "Product category name must not exceed 100 characters.")
    .transform((value) => value.replace(/\s+/g, " ")),
  description: z
    .string()
    .trim()
    .max(1000, "Product category description must not exceed 1000 characters."),
});

/**
 * @param {EditProductCategoryDetailsDialogProps} props
 */
export function EditProductCategoryDetailsDialog({
  open,
  category,
  isPending,
  error,
  onClose,
  onSubmit,
  onReload,
}) {
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm({
    resolver: zodResolver(editProductCategoryDetailsSchema),
    defaultValues: createDefaultValues(category),
  });

  useEffect(() => {
    if (open) {
      reset(createDefaultValues(category));
    }
  }, [category, open, reset]);

  async function handleValidSubmit(values) {
    try {
      await onSubmit({
        name: values.name,
        description: values.description === "" ? null : values.description,
        rowVersion: category.rowVersion,
      });
      onClose();
    } catch {
      // Mutation hook exposes the error while the dialog keeps user input.
    }
  }

  function handleDialogClose() {
    if (!isPending) {
      onClose();
    }
  }

  const isConcurrencyConflict =
    error instanceof ApiError && error.status === 409;

  return (
    <Dialog
      open={open}
      onClose={handleDialogClose}
      fullWidth
      maxWidth="sm"
      aria-labelledby="edit-product-category-dialog-title"
    >
      <Box
        component="form"
        noValidate
        onSubmit={handleSubmit(handleValidSubmit)}
      >
        <DialogTitle id="edit-product-category-dialog-title">
          Edit product category
        </DialogTitle>

        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Category code"
              value={category.code}
              fullWidth
              disabled
              helperText="A category code cannot be changed after creation."
            />

            <TextField
              label="Category name"
              required
              fullWidth
              autoFocus
              disabled={isPending}
              error={Boolean(errors.name)}
              helperText={errors.name?.message}
              {...register("name")}
            />

            <TextField
              label="Description"
              fullWidth
              multiline
              minRows={3}
              disabled={isPending}
              error={Boolean(errors.description)}
              helperText={errors.description?.message}
              {...register("description")}
            />

            {error && (
              <Alert
                severity="error"
                action={
                  isConcurrencyConflict ? (
                    <Button
                      color="inherit"
                      size="small"
                      disabled={isPending}
                      onClick={onReload}
                    >
                      Reload category
                    </Button>
                  ) : undefined
                }
              >
                {getUpdateErrorMessage(error)}
              </Alert>
            )}
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
            {isPending ? "Saving..." : "Save changes"}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}

function createDefaultValues(category) {
  return {
    name: category.name,
    description: category.description ?? "",
  };
}

function getUpdateErrorMessage(error) {
  if (error instanceof ApiError && error.status === 409) {
    return "The product category was modified by another user. Reload it and try again.";
  }

  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to update the product category.";
}
