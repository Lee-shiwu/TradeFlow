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
import { useUpdateProductCategoryDetails } from "../hooks/useUpdateProductCategoryDetails";

export function ProductCategoryDetailsPage() {
  const { productCategoryId } = useParams();
  const [editDialogOpen, setEditDialogOpen] = useState(false);
  const categoryQuery = useProductCategory(productCategoryId);
  const updateCategory = useUpdateProductCategoryDetails(productCategoryId);
  const category = categoryQuery.data;

  const isNotFound =
    categoryQuery.error instanceof ApiError &&
    categoryQuery.error.status === 404;

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
              <Button
                variant="contained"
                onClick={() => {
                  updateCategory.reset();
                  setEditDialogOpen(true);
                }}
              >
                Edit category
              </Button>
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

function formatDate(value) {
  if (value === null) {
    return "—";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}
