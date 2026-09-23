/**
 * @typedef {import("../api/createProductCategoryTypes.js").CreateProductCategoryRequest}
 * CreateProductCategoryRequest
 */

/**
 * @typedef {import("../api/createProductCategoryTypes.js").CreateProductCategoryResponse}
 * CreateProductCategoryResponse
 */

/**
 * @typedef {Object} CreateProductCategoryDialogProps
 * @property {boolean} open
 * @property {boolean} isPending
 * @property {unknown} error
 * @property {() => void} onClose
 * @property {(request: CreateProductCategoryRequest) => Promise<CreateProductCategoryResponse>} onSubmit
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

const codePattern = /^[A-Z0-9-]+$/;

const createProductCategorySchema = z.object({
  code: z
    .string()
    .trim()
    .min(1, "Product category code is required.")
    .max(50, "Product category code must not exceed 50 characters.")
    .transform((value) => value.toUpperCase())
    .refine(
      (value) => codePattern.test(value),
      "Product category code can only contain letters, numbers, and hyphens.",
    ),
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

const emptyFormValues = {
  code: "",
  name: "",
  description: "",
};

/**
 * @param {CreateProductCategoryDialogProps} props
 */
export function CreateProductCategoryDialog({
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
    resolver: zodResolver(createProductCategorySchema),
    defaultValues: emptyFormValues,
  });

  useEffect(() => {
    if (open) {
      reset(emptyFormValues);
    }
  }, [open, reset]);

  async function handleValidSubmit(values) {
    try {
      const createdCategory = await onSubmit({
        code: values.code,
        name: values.name,
        description: values.description === "" ? null : values.description,
      });

      onClose();
      return createdCategory;
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
      aria-labelledby="create-product-category-dialog-title"
    >
      <Box
        component="form"
        noValidate
        onSubmit={handleSubmit(handleValidSubmit)}
      >
        <DialogTitle id="create-product-category-dialog-title">
          Create product category
        </DialogTitle>

        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Category code"
              required
              fullWidth
              autoFocus
              disabled={isPending}
              error={Boolean(errors.code)}
              helperText={
                errors.code?.message ?? "Letters, numbers, and hyphens only."
              }
              {...register("code")}
            />

            <TextField
              label="Category name"
              required
              fullWidth
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
              <Alert severity="error">{getCreateErrorMessage(error)}</Alert>
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
            {isPending ? "Creating..." : "Create category"}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}

function getCreateErrorMessage(error) {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to create the product category.";
}
