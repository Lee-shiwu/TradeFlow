/**
 * @typedef {import("../api/createProductTypes.js").CreateProductRequest}
 * CreateProductRequest
 */

/**
 * @typedef {import("../api/createProductTypes.js").CreateProductResponse}
 * CreateProductResponse
 */

/**
 * @typedef {import("../api/productReferenceDataTypes.js").ProductReferenceData}
 * ProductReferenceData
 */

/**
 * @typedef {Object} CreateProductDialogProps
 * @property {boolean} open
 * @property {boolean} isPending
 * @property {unknown} error
 * @property {ProductReferenceData} referenceData
 * @property {boolean} isReferenceDataPending
 * @property {unknown} referenceDataError
 * @property {() => void} onClose
 * @property {(request: CreateProductRequest) =>
 * Promise<CreateProductResponse>} onSubmit
 * @property {() => void} onRetryReferenceData
 */

import { zodResolver } from "@hookform/resolvers/zod";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useEffect } from "react";
import { Controller, useForm } from "react-hook-form";
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
  unitOfMeasureId: requiredGuid(
    "Unit of measure is required.",
    "Unit of measure must be selected.",
  ),
  productCategoryId: z
    .string()
    .trim()
    .refine(
      (value) =>
        value === "" || (guidPattern.test(value) && value !== emptyGuid),
      "Product category must be selected.",
    )
    .transform((value) => (value === "" ? null : value)),
  taxCategoryId: requiredGuid(
    "Tax category is required.",
    "Tax category must be selected.",
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

const emptyReferenceData = {
  unitsOfMeasure: [],
  productCategories: [],
  taxCategories: [],
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
  referenceData = emptyReferenceData,
  isReferenceDataPending = false,
  referenceDataError = null,
  onClose,
  onSubmit,
  onRetryReferenceData = () => {},
}) {
  const {
    register,
    control,
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
      return undefined;
    }
  }

  function handleDialogClose() {
    if (!isPending) {
      onClose();
    }
  }

  const formDisabled = isPending || isReferenceDataPending;

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
            {isReferenceDataPending && (
              <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
                <CircularProgress size={20} />
                <Typography>Loading product options...</Typography>
              </Stack>
            )}

            {referenceDataError && (
              <Alert
                severity="error"
                action={
                  <Button
                    color="inherit"
                    size="small"
                    onClick={onRetryReferenceData}
                  >
                    Retry
                  </Button>
                }
              >
                {getReferenceDataErrorMessage(referenceDataError)}
              </Alert>
            )}

            <TextField
              label="Product SKU"
              required
              fullWidth
              autoFocus
              disabled={formDisabled}
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
              disabled={formDisabled}
              error={Boolean(errors.name)}
              helperText={errors.name?.message}
              {...register("name")}
            />

            <TextField
              label="Description"
              fullWidth
              multiline
              minRows={3}
              disabled={formDisabled}
              error={Boolean(errors.description)}
              helperText={errors.description?.message}
              {...register("description")}
            />

            <ReferenceDataSelect
              name="unitOfMeasureId"
              label="Unit of measure"
              required
              control={control}
              items={referenceData.unitsOfMeasure}
              disabled={formDisabled}
              error={errors.unitOfMeasureId}
            />

            <ReferenceDataSelect
              name="productCategoryId"
              label="Product category"
              control={control}
              items={referenceData.productCategories}
              disabled={formDisabled}
              error={errors.productCategoryId}
              emptyOptionLabel="No product category"
            />

            <ReferenceDataSelect
              name="taxCategoryId"
              label="Tax category"
              required
              control={control}
              items={referenceData.taxCategories}
              disabled={formDisabled}
              error={errors.taxCategoryId}
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

          <Button
            type="submit"
            variant="contained"
            disabled={
              formDisabled ||
              Boolean(referenceDataError) ||
              referenceData.unitsOfMeasure.length === 0 ||
              referenceData.taxCategories.length === 0
            }
          >
            {isPending ? "Creating..." : "Create product"}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
}

function ReferenceDataSelect({
  name,
  label,
  required = false,
  control,
  items,
  disabled,
  error,
  emptyOptionLabel,
}) {
  return (
    <Controller
      name={name}
      control={control}
      render={({ field }) => (
        <TextField
          {...field}
          select
          label={label}
          required={required}
          fullWidth
          disabled={disabled}
          error={Boolean(error)}
          helperText={error?.message}
        >
          {emptyOptionLabel && <MenuItem value="">{emptyOptionLabel}</MenuItem>}

          {items.map((item) => (
            <MenuItem key={item.id} value={item.id}>
              {formatReferenceDataItem(item)}
            </MenuItem>
          ))}
        </TextField>
      )}
    />
  );
}

function requiredGuid(requiredMessage, invalidMessage) {
  return z
    .string()
    .trim()
    .min(1, requiredMessage)
    .refine(
      (value) =>
        value === "" || (guidPattern.test(value) && value !== emptyGuid),
      invalidMessage,
    );
}

function getCreateErrorMessage(error) {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to create the product.";
}

function getReferenceDataErrorMessage(error) {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to load product options.";
}

function formatReferenceDataItem(item) {
  return `${item.code} — ${item.name}`;
}
