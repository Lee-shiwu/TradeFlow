/** @typedef {import("react").ReactNode} ReactNode */

/**
 * @typedef {Object} ProductCategoryDetailFieldProps
 * @property {string} label
 * @property {ReactNode} value
 */

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
import { useState } from "react";
import { Link as RouterLink, useParams } from "react-router-dom";
import { ApiError } from "../../../shared/api/ApiError";
import { EditProductCategoryDetailsDialog } from "../components/EditProductCategoryDetailsDialog";
import { useProductCategory } from "../hooks/useProductCategory";
import { useProductCategoryStatusAction } from "../hooks/useProductCategoryStatusAction";
import { useUpdateProductCategoryDetails } from "../hooks/useUpdateProductCategoryDetails";

export function ProductCategoryDetailsPage() {
  const { productCategoryId } = useParams();
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const [pendingStatusAction, setPendingStatusAction] = useState(null);
  const categoryQuery = useProductCategory(productCategoryId);
  const updateCategory = useUpdateProductCategoryDetails(productCategoryId);
  const statusAction = useProductCategoryStatusAction(productCategoryId);
  const category = categoryQuery.data;

  const isNotFound =
    categoryQuery.error instanceof ApiError &&
    categoryQuery.error.status === 404;

  const isConcurrencyConflict =
    statusAction.error instanceof ApiError && statusAction.error.status === 409;

  const isActivateAction = pendingStatusAction === "activate";

  function handleOpenStatusDialog() {
    if (!category) {
      return;
    }

    statusAction.reset();
    setPendingStatusAction(
      category.status === "Active" ? "deactivate" : "activate",
    );
  }

  function handleCloseStatusDialog() {
    if (statusAction.isPending) {
      return;
    }

    statusAction.reset();
    setPendingStatusAction(null);
  }

  function handleConfirmStatusAction() {
    if (!category || !pendingStatusAction) {
      return;
    }

    statusAction.mutate(
      {
        action: pendingStatusAction,
        rowVersion: category.rowVersion,
      },
      {
        onSuccess: () => setPendingStatusAction(null),
      },
    );
  }

  function handleReloadCategory() {
    statusAction.reset();
    setPendingStatusAction(null);
    void categoryQuery.refetch();
  }

  return (
    <Box component="main" sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={2}
          sx={{
            justifyContent: "space-between",
            alignItems: { xs: "flex-start", sm: "center" },
          }}
        >
          <Typography component="h1" variant="h4">
            Product category details
          </Typography>

          <Stack direction="row" spacing={1}>
            {category && (
              <>
                <Button
                  variant="outlined"
                  disabled={
                    statusAction.isPending ||
                    updateCategory.isPending ||
                    categoryQuery.isFetching
                  }
                  onClick={() => {
                    updateCategory.reset();
                    setEditDialogOpen(true);
                  }}
                >
                  Edit category
                </Button>

                <Button
                  variant={
                    category.status === "Active" ? "outlined" : "contained"
                  }
                  color={category.status === "Active" ? "error" : "primary"}
                  disabled={
                    statusAction.isPending ||
                    updateCategory.isPending ||
                    categoryQuery.isFetching
                  }
                  onClick={handleOpenStatusDialog}
                >
                  {category.status === "Active"
                    ? "Deactivate category"
                    : "Activate category"}
                </Button>
              </>
            )}

            <Button
              component={RouterLink}
              to="/product-categories"
              variant="outlined"
            >
              Back to product categories
            </Button>
          </Stack>
        </Stack>

        {!productCategoryId && (
          <Alert severity="error">Product category ID is missing.</Alert>
        )}

        {productCategoryId && categoryQuery.isPending && (
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <CircularProgress size={24} />
            <Typography>Loading product category details...</Typography>
          </Stack>
        )}

        {productCategoryId &&
          categoryQuery.isFetching &&
          !categoryQuery.isPending && <LinearProgress />}

        {productCategoryId && categoryQuery.isError && isNotFound && (
          <Alert severity="info">
            The selected product category does not exist or is not available in
            your organisation.
          </Alert>
        )}

        {productCategoryId && categoryQuery.isError && !isNotFound && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                disabled={categoryQuery.isFetching}
                onClick={() => {
                  void categoryQuery.refetch();
                }}
              >
                Retry
              </Button>
            }
          >
            {getErrorMessage(categoryQuery.error)}
          </Alert>
        )}

        {category && !isNotFound && (
          <Paper sx={{ p: 3 }}>
            <Stack spacing={3}>
              <Typography component="h2" variant="h6">
                Category information
              </Typography>

              <Stack component="dl" spacing={2} sx={{ m: 0 }}>
                <ProductCategoryDetailField
                  label="Product category ID"
                  value={category.productCategoryId}
                />

                <ProductCategoryDetailField
                  label="Code"
                  value={category.code}
                />

                <ProductCategoryDetailField
                  label="Name"
                  value={category.name}
                />

                <ProductCategoryDetailField
                  label="Description"
                  value={category.description || "—"}
                />

                <ProductCategoryDetailField
                  label="Status"
                  value={
                    <Chip
                      label={category.status}
                      color={
                        category.status === "Active" ? "success" : "default"
                      }
                      size="small"
                    />
                  }
                />

                <ProductCategoryDetailField
                  label="Created at"
                  value={formatDate(category.createdAt)}
                />

                <ProductCategoryDetailField
                  label="Created by"
                  value={category.createdBy}
                />

                <ProductCategoryDetailField
                  label="Last modified at"
                  value={formatDate(category.lastModifiedAt)}
                />

                <ProductCategoryDetailField
                  label="Last modified by"
                  value={category.lastModifiedBy ?? "—"}
                />
              </Stack>
            </Stack>
          </Paper>
        )}

        {category && (
          <EditProductCategoryDetailsDialog
            open={editDialogOpen}
            category={category}
            isPending={updateCategory.isPending}
            error={updateCategory.error}
            onClose={() => {
              setEditDialogOpen(false);
              updateCategory.reset();
            }}
            onSubmit={(request) => updateCategory.mutateAsync(request)}
            onReload={() => {
              setEditDialogOpen(false);
              updateCategory.reset();
              void categoryQuery.refetch();
            }}
          />
        )}
      </Stack>

      <Dialog
        open={pendingStatusAction !== null}
        onClose={handleCloseStatusDialog}
        aria-labelledby="product-category-status-dialog-title"
      >
        <DialogTitle id="product-category-status-dialog-title">
          {isActivateAction
            ? "Activate product category"
            : "Deactivate product category"}
        </DialogTitle>

        <DialogContent>
          <DialogContentText>
            {isActivateAction
              ? `Activate ${category?.name ?? "this category"}? It will become available for product assignment.`
              : `Deactivate ${category?.name ?? "this category"}? It will no longer be available for new product assignments.`}
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
                    onClick={handleReloadCategory}
                  >
                    Reload category
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
                ? "Activate category"
                : "Deactivate category"}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

/**
 * @param {ProductCategoryDetailFieldProps} props
 */
function ProductCategoryDetailField({ label, value }) {
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

function getErrorMessage(error) {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to load product category details.";
}

function getStatusActionErrorMessage(error) {
  if (error instanceof ApiError && error.status === 409) {
    return "The product category was changed by another request. Reload it before trying again.";
  }

  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to change the product category status.";
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
