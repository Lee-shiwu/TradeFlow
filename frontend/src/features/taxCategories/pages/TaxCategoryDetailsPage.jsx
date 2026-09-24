/** @typedef {import("react").ReactNode} ReactNode */

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
import { Link as RouterLink, useParams } from "react-router-dom";
import { ApiError } from "../../../shared/api/ApiError";
import { useTaxCategory } from "../hooks/useTaxCategory";

export function TaxCategoryDetailsPage() {
  const { taxCategoryId } = useParams();
  const categoryQuery = useTaxCategory(taxCategoryId);
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
            Tax category details
          </Typography>

          <Button
            component={RouterLink}
            to="/tax-categories"
            variant="outlined"
          >
            Back to tax categories
          </Button>
        </Stack>

        {!taxCategoryId && (
          <Alert severity="error">Tax category ID is missing.</Alert>
        )}

        {taxCategoryId && categoryQuery.isPending && (
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <CircularProgress size={24} />
            <Typography>Loading tax category details...</Typography>
          </Stack>
        )}

        {taxCategoryId &&
          categoryQuery.isFetching &&
          !categoryQuery.isPending && <LinearProgress />}

        {taxCategoryId && categoryQuery.isError && isNotFound && (
          <Alert severity="info">
            The selected tax category does not exist.
          </Alert>
        )}

        {taxCategoryId && categoryQuery.isError && !isNotFound && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                disabled={categoryQuery.isFetching}
                onClick={() => void categoryQuery.refetch()}
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
                Tax information
              </Typography>

              <Stack component="dl" spacing={2} sx={{ m: 0 }}>
                <DetailField
                  label="Tax category ID"
                  value={category.taxCategoryId}
                />
                <DetailField label="Code" value={category.code} />
                <DetailField label="Name" value={category.name} />
                <DetailField
                  label="Description"
                  value={category.description || "—"}
                />
                <DetailField
                  label="Treatment"
                  value={formatTreatment(category.treatment)}
                />
                <DetailField label="Rate" value={formatRate(category.rate)} />
                <DetailField
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
                <DetailField
                  label="Effective from"
                  value={formatDateOnly(category.effectiveFrom)}
                />
                <DetailField
                  label="Effective to"
                  value={formatDateOnly(category.effectiveTo)}
                />
              </Stack>
            </Stack>
          </Paper>
        )}
      </Stack>
    </Box>
  );
}

/**
 * @param {{ label: string, value: ReactNode }} props
 */
function DetailField({ label, value }) {
  return (
    <Box>
      <Typography component="dt" variant="body2" color="text.secondary">
        {label}
      </Typography>
      <Typography
        component="dd"
        variant="body1"
        sx={{ m: 0, mt: 0.5, overflowWrap: "anywhere", whiteSpace: "pre-wrap" }}
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

  return "Unable to load tax category details.";
}

function formatTreatment(treatment) {
  const labels = {
    StandardRated: "Standard rated",
    ZeroRated: "Zero rated",
    Exempt: "Exempt",
  };

  return labels[treatment] ?? treatment;
}

function formatRate(rate) {
  return new Intl.NumberFormat(undefined, {
    style: "percent",
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(rate);
}

function formatDateOnly(value) {
  if (value === null) {
    return "No end date";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeZone: "UTC",
  }).format(new Date(`${value}T00:00:00Z`));
}
