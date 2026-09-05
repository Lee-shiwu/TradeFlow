/**
 * @typedef {import("../api/productDetailsTypes.js").ProductDetails}
 * ProductDetails
 */

/**
 * @typedef {import("../api/productReferenceDataTypes.js").ProductReferenceData}
 * ProductReferenceData
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
 * @property {ProductReferenceData} referenceData
 * @property {boolean} isReferenceDataPending
 * @property {unknown} referenceDataError
 * @property {() => void} onClose
 * @property {(request: UpdateProductDetailsRequest) => Promise<void>} onSubmit
 * @property {() => void} onReload
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
      "Product category must be selected.",
    )
    .transform((value) => (value === "" ? null : value)),
  taxCategoryId: z
    .string()
    .trim()
    .min(1, "Tax category is required.")
    .refine(
      (value) =>
        value === "" || (guidPattern.test(value) && value !== emptyGuid),
      "Tax category must be selected.",
    ),
});

const emptyReferenceData = {
  unitsOfMeasure: [],
  productCategories: [],
  taxCategories: [],
};

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
  referenceData = emptyReferenceData,
  isReferenceDataPending = false,
  referenceDataError = null,
  onClose,
  onSubmit,
  onReload,
  onRetryReferenceData = () => {},
}) {
  const {
    register,
    control,
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
      // Mutation Hook 保存错误，对话框保持打开。
    }
  }

  function handleDialogClose() {
    if (!isPending) {
      onClose();
    }
  }

  const isConcurrencyConflict =
    error instanceof ApiError && error.status === 409;

  const formDisabled = isPending || isReferenceDataPending;

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
              name="productCategoryId"
              label="Product category"
              control={control}
              items={referenceData.productCategories}
              currentValue={product.productCategoryId}
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
              currentValue={product.taxCategoryId}
              disabled={formDisabled}
              error={errors.taxCategoryId}
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

          <Button
            type="submit"
            variant="contained"
            disabled={
              formDisabled ||
              Boolean(referenceDataError) ||
              referenceData.taxCategories.length === 0
            }
          >
            {isPending ? "Saving..." : "Save changes"}
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
  currentValue,
  disabled,
  error,
  emptyOptionLabel,
}) {
  const currentValueIsUnavailable =
    Boolean(currentValue) && !items.some((item) => item.id === currentValue);

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

          {currentValueIsUnavailable && (
            <MenuItem value={currentValue} disabled>
              Current selection is unavailable
            </MenuItem>
          )}

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

function getReferenceDataErrorMessage(error) {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to load product options.";
}

function formatReferenceDataItem(item) {
  return `${item.code} — ${item.name}`;
}
