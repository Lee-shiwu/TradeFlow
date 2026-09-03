/**
 * @typedef {import("../api/productDetailsTypes.js").ProductDetails}
 * ProductDetails
 */

/**
 * @typedef {import("../api/updateProductDetailsTypes.js").UpdateProductDetailsRequest}
 * UpdateProductDetailsRequest
 */

/**
 * @typedef {Object} EditProductDetailsDialogProps
 * @property {boolean} open
 * @property {ProductDetails} product
 * @property {boolean} isPending
 * @property {unknown} error
 * @property {() => void} onClose
 * @property {(request: UpdateProductDetailsRequest) => Promise<void>}
 * onSubmit
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

const emptyGuid = "00000000-0000-0000-0000-000000000000";

const guidPattern =
  /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

const editProductDetailsSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Product name is required.")
    .max(100, "Product name must not exceed 100 characters."),

  description: z
    .string()
    .trim()
    .max(1000, "Description must not exceed 1000 characters."),

  productCategoryId: z
    .string()
    .trim()
    .refine(
      (value) =>
        value === "" || (guidPattern.test(value) && value !== emptyGuid),
      "Product category ID must be a valid non-empty GUID.",
    )
    .transform((value) => (value === "" ? null : value)),

  taxCategoryId: z
    .string()
    .trim()
    .min(1, "Tax category ID is required.")
    .refine(
      (value) =>
        value === "" || (guidPattern.test(value) && value !== emptyGuid),
      "Tax category ID must be a valid non-empty GUID.",
    ),
});

/**
 * 修改商品详情的表单对话框。
 *
 * @param {EditProductDetailsDialogProps} props
 */
export function EditProductDetailsDialog({
  open,
  product,
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
    resolver: zodResolver(editProductDetailsSchema),
    defaultValues: createDefaultValues(product),
  });

  useEffect(() => {
    if (open) {
      reset(createDefaultValues(product));
    }
  }, [open, product, reset]);

  async function handleValidSubmit(values) {
    const request = {
      name: values.name,
      description: values.description === "" ? null : values.description,
      productCategoryId: values.productCategoryId,
      taxCategoryId: values.taxCategoryId,
      rowVersion: product.rowVersion,
    };

    try {
      await onSubmit(request);
      onClose();
    } catch {
      // Mutation Hook 保存错误状态，
      // 对话框通过 error 属性显示错误。
    }
  }

  function handleDialogClose() {
    if (isPending) {
      return;
    }

    onClose();
  }

  const isConcurrencyConflict =
    error instanceof ApiError && error.status === 409;

  return (
    <Dialog
      open={open}
      onClose={handleDialogClose}
      fullWidth
      maxWidth="sm"
      aria-labelledby="edit-product-dialog-title"
    >
      <Box
        component="form"
        noValidate
        onSubmit={handleSubmit(handleValidSubmit)}
      >
        <DialogTitle id="edit-product-dialog-title">Edit product</DialogTitle>

        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Product name"
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

            <TextField
              label="Product category ID"
              fullWidth
              disabled={isPending}
              error={Boolean(errors.productCategoryId)}
              helperText={
                errors.productCategoryId?.message ??
                "Optional. Leave empty to remove the product category."
              }
              {...register("productCategoryId")}
            />

            <TextField
              label="Tax category ID"
              required
              fullWidth
              disabled={isPending}
              error={Boolean(errors.taxCategoryId)}
              helperText={errors.taxCategoryId?.message}
              {...register("taxCategoryId")}
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
                      Reload product
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

function createDefaultValues(product) {
  return {
    name: product.name,
    description: product.description ?? "",
    productCategoryId: product.productCategoryId ?? "",
    taxCategoryId: product.taxCategoryId,
  };
}

function getUpdateErrorMessage(error) {
  if (error instanceof ApiError && error.status === 409) {
    return "The product was modified by another user. Reload the product and try again.";
  }

  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to update the product.";
}
