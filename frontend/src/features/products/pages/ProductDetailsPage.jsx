/** @typedef {import("react").ReactNode} ReactNode */

/**
 * @typedef {Object} ProductDetailFieldProps
 * @property {string} label
 * @property {ReactNode} value
 */

import { useState } from "react";
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  LinearProgress,
  Paper,
  Stack,
  Typography,
} from "@mui/material";
import { Link as RouterLink, useParams } from "react-router-dom";
import { ApiError } from "../../../shared/api/ApiError";
import { EditProductDetailsDialog } from "../components/EditProductDetailsDialog";
import { useProduct } from "../hooks/useProduct";
import { useProductStatusAction } from "../hooks/useProductStatusAction";
import { useUpdateProductDetails } from "../hooks/useUpdateProductDetails";

export function ProductDetailsPage() {
  const { productId } = useParams();

  const productQuery = useProduct(productId);

  const statusAction = useProductStatusAction(productId ?? "");
  const updateMutation = useUpdateProductDetails(productId ?? "");

  const [pendingAction, setPendingAction] = useState(null);
  const [editDialogOpen, setEditDialogOpen] = useState(false);

  const product = productQuery.data;

  const isNotFound =
    productQuery.error instanceof ApiError && productQuery.error.status === 404;

  const isConcurrencyConflict =
    statusAction.error instanceof ApiError && statusAction.error.status === 409;

  const isActivateAction = pendingAction === "activate";

  function handleOpenStatusDialog() {
    if (!product) {
      return;
    }

    statusAction.reset();

    const nextAction = product.status === "Active" ? "deactivate" : "activate";

    setPendingAction(nextAction);
  }

  function handleCloseStatusDialog() {
    if (statusAction.isPending) {
      return;
    }

    statusAction.reset();
    setPendingAction(null);
  }

  function handleConfirmStatusAction() {
    if (!product || !pendingAction) {
      return;
    }

    statusAction.mutate(
      {
        action: pendingAction,
        rowVersion: product.rowVersion,
      },
      {
        onSuccess: () => {
          setPendingAction(null);
        },
      },
    );
  }

  function handleReloadProduct() {
    statusAction.reset();
    setPendingAction(null);

    void productQuery.refetch();
  }

  function handleOpenEditDialog() {
    if (!product) {
      return;
    }

    updateMutation.reset();
    setEditDialogOpen(true);
  }

  function handleCloseEditDialog() {
    if (updateMutation.isPending) {
      return;
    }

    updateMutation.reset();
    setEditDialogOpen(false);
  }

  async function handleUpdateProduct(request) {
    await updateMutation.mutateAsync(request);
  }

  function handleReloadUpdatedProduct() {
    updateMutation.reset();
    setEditDialogOpen(false);

    void productQuery.refetch();
  }

  return (
    <Box component="main" sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack
          direction={{
            xs: "column",
            sm: "row",
          }}
          spacing={2}
          sx={{
            justifyContent: "space-between",
            alignItems: {
              xs: "flex-start",
              sm: "center",
            },
          }}
        >
          <Typography component="h1" variant="h4">
            Product details
          </Typography>

          <Button component={RouterLink} to="/products" variant="outlined">
            Back to products
          </Button>
        </Stack>

        {!productId && <Alert severity="error">Product ID is missing.</Alert>}

        {productId && productQuery.isPending && (
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <CircularProgress size={24} />

            <Typography>Loading product details...</Typography>
          </Stack>
        )}

        {productId && productQuery.isFetching && !productQuery.isPending && (
          <LinearProgress />
        )}

        {productId && productQuery.isError && isNotFound && (
          <Alert severity="info">
            The selected product does not exist or is not available in your
            organisation.
          </Alert>
        )}

        {productId && productQuery.isError && !isNotFound && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                disabled={productQuery.isFetching}
                onClick={() => {
                  void productQuery.refetch();
                }}
              >
                Retry
              </Button>
            }
          >
            {getProductQueryErrorMessage(productQuery.error)}
          </Alert>
        )}

        {product && !isNotFound && (
          <Paper sx={{ p: 3 }}>
            <Stack spacing={3}>
              <Stack
                direction={{
                  xs: "column",
                  sm: "row",
                }}
                spacing={2}
                sx={{
                  justifyContent: "space-between",
                  alignItems: {
                    xs: "stretch",
                    sm: "center",
                  },
                }}
              >
                <Typography component="h2" variant="h6">
                  Product information
                </Typography>

                <Stack direction="row" spacing={1}>
                  <Button
                    variant="outlined"
                    disabled={
                      statusAction.isPending ||
                      updateMutation.isPending ||
                      productQuery.isFetching
                    }
                    onClick={handleOpenEditDialog}
                  >
                    Edit product
                  </Button>

                  <Button
                    variant={
                      product.status === "Active" ? "outlined" : "contained"
                    }
                    color={product.status === "Active" ? "error" : "primary"}
                    disabled={
                      statusAction.isPending ||
                      updateMutation.isPending ||
                      productQuery.isFetching
                    }
                    onClick={handleOpenStatusDialog}
                  >
                    {product.status === "Active"
                      ? "Deactivate product"
                      : "Activate product"}
                  </Button>
                </Stack>
              </Stack>

              <Stack component="dl" spacing={2} sx={{ m: 0 }}>
                <ProductDetailField
                  label="Product ID"
                  value={product.productId}
                />

                <ProductDetailField label="SKU" value={product.sku} />

                <ProductDetailField label="Name" value={product.name} />

                <ProductDetailField
                  label="Description"
                  value={product.description || "—"}
                />

                <ProductDetailField
                  label="Status"
                  value={
                    <Chip
                      label={product.status}
                      color={
                        product.status === "Active" ? "success" : "default"
                      }
                      size="small"
                    />
                  }
                />

                <ProductDetailField
                  label="Unit of measure ID"
                  value={product.unitOfMeasureId}
                />

                <ProductDetailField
                  label="Product category ID"
                  value={product.productCategoryId ?? "—"}
                />

                <ProductDetailField
                  label="Tax category ID"
                  value={product.taxCategoryId}
                />

                <ProductDetailField
                  label="Created at"
                  value={formatDate(product.createdAt)}
                />

                <ProductDetailField
                  label="Created by"
                  value={product.createdBy}
                />

                <ProductDetailField
                  label="Last modified at"
                  value={formatDate(product.lastModifiedAt)}
                />

                <ProductDetailField
                  label="Last modified by"
                  value={product.lastModifiedBy ?? "—"}
                />
              </Stack>
            </Stack>
          </Paper>
        )}
      </Stack>

      <Dialog
        open={pendingAction !== null}
        onClose={handleCloseStatusDialog}
        aria-labelledby="product-status-dialog-title"
      >
        <DialogTitle id="product-status-dialog-title">
          {isActivateAction ? "Activate product" : "Deactivate product"}
        </DialogTitle>

        <DialogContent>
          <DialogContentText>
            {isActivateAction
              ? `Activate ${product?.name ?? "this product"}? The product will become available for business operations.`
              : `Deactivate ${product?.name ?? "this product"}? The product will no longer be available for new business operations.`}
          </DialogContentText>

          {statusAction.isError && (
            <Alert
              severity="error"
              sx={{ mt: 2 }}
              action={
                isConcurrencyConflict ? (
                  <Button
                    color="inherit"
                    size="small"
                    onClick={handleReloadProduct}
                  >
                    Reload product
                  </Button>
                ) : undefined
              }
            >
              {getStatusActionErrorMessage(statusAction.error)}
            </Alert>
          )}
        </DialogContent>

        <DialogActions>
          <Button
            disabled={statusAction.isPending}
            onClick={handleCloseStatusDialog}
          >
            Cancel
          </Button>

          <Button
            variant="contained"
            color={isActivateAction ? "primary" : "error"}
            disabled={statusAction.isPending}
            onClick={handleConfirmStatusAction}
          >
            {statusAction.isPending
              ? "Saving..."
              : isActivateAction
                ? "Activate product"
                : "Deactivate product"}
          </Button>
        </DialogActions>
      </Dialog>

      {product && (
        <EditProductDetailsDialog
          open={editDialogOpen}
          product={product}
          isPending={updateMutation.isPending}
          error={updateMutation.error}
          onClose={handleCloseEditDialog}
          onSubmit={handleUpdateProduct}
          onReload={handleReloadUpdatedProduct}
        />
      )}
    </Box>
  );
}

function ProductDetailField({ label, value }) {
  return (
    <Box>
      <Typography component="dt" variant="body2" color="text.secondary">
        {label}
      </Typography>

      <Typography
        component="dd"
        variant="body1"
        sx={{
          m: 0,
          mt: 0.5,
          overflowWrap: "anywhere",
          whiteSpace: "pre-wrap",
        }}
      >
        {value}
      </Typography>
    </Box>
  );
}

function getProductQueryErrorMessage(error) {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to load product details.";
}

function getStatusActionErrorMessage(error) {
  if (error instanceof ApiError && error.status === 409) {
    return "The product was changed by another request. Reload the product before trying again.";
  }

  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to change the product status.";
}

function formatDate(value) {
  if (value === null) {
    return "—";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}
