import { useState } from "react";
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  FormControl,
  InputLabel,
  LinearProgress,
  MenuItem,
  Pagination,
  Paper,
  Select,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from "@mui/material";
import { ApiError } from "../../../shared/api/ApiError";
import { CreateTaxCategoryDialog } from "../components/CreateTaxCategoryDialog";
import { useCreateTaxCategory } from "../hooks/useCreateTaxCategory";
import { useTaxCategories } from "../hooks/useTaxCategories";
import { Link as RouterLink } from "react-router-dom";

export function TaxCategoryListPage() {
  const [searchInput, setSearchInput] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [status, setStatus] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const pageSize = 20;
  const createMutation = useCreateTaxCategory();

  const taxCategoriesQuery = useTaxCategories({
    search: appliedSearch,
    status: status || undefined,
    pageNumber,
    pageSize,
  });

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
            Tax categories
          </Typography>

          <Button
            variant="contained"
            onClick={() => {
              createMutation.reset();
              setCreateDialogOpen(true);
            }}
          >
            Create tax category
          </Button>
        </Stack>

        <Typography color="text.secondary">
          Review GST treatment, rates, and effective periods available to
          products.
        </Typography>

        <Stack
          component="form"
          direction={{ xs: "column", sm: "row" }}
          spacing={2}
          onSubmit={(event) => {
            event.preventDefault();
            setAppliedSearch(searchInput.trim());
            setPageNumber(1);
          }}
        >
          <TextField
            label="Search"
            placeholder="Search by code or name"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
          />

          <FormControl sx={{ minWidth: 180 }}>
            <InputLabel id="tax-category-status-label">Status</InputLabel>
            <Select
              labelId="tax-category-status-label"
              label="Status"
              value={status}
              onChange={(event) => {
                setStatus(event.target.value);
                setPageNumber(1);
              }}
            >
              <MenuItem value="">All statuses</MenuItem>
              <MenuItem value="Active">Active</MenuItem>
              <MenuItem value="Inactive">Inactive</MenuItem>
            </Select>
          </FormControl>

          <Button type="submit" variant="contained">
            Search
          </Button>
        </Stack>

        {taxCategoriesQuery.isFetching && !taxCategoriesQuery.isPending && (
          <LinearProgress />
        )}

        {taxCategoriesQuery.isPending && (
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <CircularProgress size={24} />
            <Typography>Loading tax categories...</Typography>
          </Stack>
        )}

        {taxCategoriesQuery.isError && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                disabled={taxCategoriesQuery.isFetching}
                onClick={() => void taxCategoriesQuery.refetch()}
              >
                Retry
              </Button>
            }
          >
            {getErrorMessage(taxCategoriesQuery.error)}
          </Alert>
        )}

        {taxCategoriesQuery.data &&
          taxCategoriesQuery.data.items.length === 0 && (
            <Alert severity="info">No tax categories found.</Alert>
          )}

        {taxCategoriesQuery.data &&
          taxCategoriesQuery.data.items.length > 0 && (
            <Stack spacing={2}>
              <Typography>
                Total tax categories: {taxCategoriesQuery.data.totalCount}
              </Typography>

              <TableContainer component={Paper}>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Code</TableCell>
                      <TableCell>Name</TableCell>
                      <TableCell>Treatment</TableCell>
                      <TableCell>Rate</TableCell>
                      <TableCell>Status</TableCell>
                      <TableCell>Effective from</TableCell>
                      <TableCell>Effective to</TableCell>
                      <TableCell align="right">Actions</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {taxCategoriesQuery.data.items.map((category) => (
                      <TableRow key={category.taxCategoryId} hover>
                        <TableCell>{category.code}</TableCell>
                        <TableCell>{category.name}</TableCell>
                        <TableCell>
                          {formatTreatment(category.treatment)}
                        </TableCell>
                        <TableCell>{formatRate(category.rate)}</TableCell>
                        <TableCell>
                          <Chip
                            label={category.status}
                            color={
                              category.status === "Active"
                                ? "success"
                                : "default"
                            }
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          {formatDateOnly(category.effectiveFrom)}
                        </TableCell>
                        <TableCell>
                          {formatDateOnly(category.effectiveTo)}
                        </TableCell>
                        <TableCell align="right">
                          <Button
                            component={RouterLink}
                            to={`/tax-categories/${category.taxCategoryId}`}
                            size="small"
                          >
                            View details
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>

              {taxCategoriesQuery.data.totalPages > 0 && (
                <Stack direction="row" sx={{ justifyContent: "center" }}>
                  <Pagination
                    page={pageNumber}
                    count={taxCategoriesQuery.data.totalPages}
                    disabled={taxCategoriesQuery.isFetching}
                    color="primary"
                    onChange={(_, selectedPage) => setPageNumber(selectedPage)}
                  />
                </Stack>
              )}
            </Stack>
          )}
      </Stack>

      <CreateTaxCategoryDialog
        open={createDialogOpen}
        isPending={createMutation.isPending}
        error={createMutation.error}
        onClose={() => {
          if (!createMutation.isPending) {
            createMutation.reset();
            setCreateDialogOpen(false);
          }
        }}
        onSubmit={(request) => createMutation.mutateAsync(request)}
      />
    </Box>
  );
}

function getErrorMessage(error) {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to load tax categories.";
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
