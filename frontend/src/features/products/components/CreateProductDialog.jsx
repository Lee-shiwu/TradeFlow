/**
 * @typedef {import("../api/createProductTypes.js").CreateProductRequest}
 * CreateProductRequest
 */

/**
 * @typedef {import("../api/createProductTypes.js").CreateProductResponse}
 * CreateProductResponse
 */

/**
 * @typedef {Object} CreateProductDialogProps
 * @property {boolean} open
 * @property {boolean} isPending
 * @property {unknown} error
 * @property {() => void} onClose
 * @property {(request: CreateProductRequest) =>
 * Promise<CreateProductResponse>} onSubmit
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

const skuPattern = /^[A-Z0-9-]+$/;

const createProductSchema = z.object({
  sku: z
    .string()
    .trim()
    .min(1, "Product SKU is required.")
    .max(50, "Product SKU must not exceed 50 characters.")
    .transform((value) => value.toUpperCase())
    .refine(
      (value) => skuPattern.test(value),
      "Product SKU can only contain letters, numbers, and hyphens.",
    ),

  name: z
    .string()
    .trim()
    .min(1, "Product name is required.")
    .max(100, "Product name must not exceed 100 characters."),

  description: z
    .string()
    .trim()
    .max(1000, "Description must not exceed 1000 characters."),

  unitOfMeasureId: z
    .string()
    .trim()
    .min(1, "Unit of measure ID is required.")
    .refine(
      (value) =>
        value === "" || (guidPattern.test(value) && value !== emptyGuid),
      "Unit of measure ID must be a valid non-empty GUID.",
    ),

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

const emptyFormValues = {
  sku: "",
  name: "",
  description: "",
  unitOfMeasureId: "",
  productCategoryId: "",
  taxCategoryId: "",
};

/**
 * 创建商品的表单对话框。
 *
 * @param {CreateProductDialogProps} props
 */
export function CreateProductDialog({
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
    resolver: zodResolver(createProductSchema),
    defaultValues: emptyFormValues,
  });

  useEffect(() => {
    if (open) {
      reset(emptyFormValues);
    }
  }, [open, reset]);

  async function handleValidSubmit(values) {
    const request = {
      sku: values.sku,
      name: values.name,
      description: values.description === "" ? null : values.description,
      unitOfMeasureId: values.unitOfMeasureId,
      productCategoryId: values.productCategoryId,
      taxCategoryId: values.taxCategoryId,
    };

    try {
      const createdProduct = await onSubmit(request);

      onClose();

      return createdProduct;
    } catch {
      // Mutation Hook 保存错误状态。
      // 对话框继续打开并通过 error 属性显示错误。
      return undefined;
    }
  }

  function handleDialogClose() {
    if (isPending) {
      return;
    }

    onClose();
  }

  return (
    <Dialog
      open={open}
      onClose={handleDialogClose}
      fullWidth
      maxWidth="sm"
      aria-labelledby="create-product-dialog-title"
    >
      <Box
        component="form"
        noValidate
        onSubmit={handleSubmit(handleValidSubmit)}
      >
        <DialogTitle id="create-product-dialog-title">
          Create product
        </DialogTitle>

        <DialogContent>
          <Stack spacing={2} sx={{ mt: 1 }}>
            <TextField
              label="Product SKU"
              required
              fullWidth
              autoFocus
              disabled={isPending}
              error={Boolean(errors.sku)}
              helperText={
                errors.sku?.message ?? "Letters, numbers, and hyphens only."
              }
              {...register("sku")}
            />

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
              label="Unit of measure ID"
              required
              fullWidth
              disabled={isPending}
              error={Boolean(errors.unitOfMeasureId)}
              helperText={errors.unitOfMeasureId?.message}
              {...register("unitOfMeasureId")}
            />

            <TextField
              label="Product category ID"
              fullWidth
              disabled={isPending}
              error={Boolean(errors.productCategoryId)}
              helperText={errors.productCategoryId?.message ?? "Optional."}
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
            {isPending ? "Creating..." : "Create product"}
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

  return "Unable to create the product.";
}
