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
import { Link as RouterLink } from "react-router-dom";
import { ApiError } from "../../../shared/api/ApiError";
import { CreateProductCategoryDialog } from "../components/CreateProductCategoryDialog";
import { useCreateProductCategory } from "../hooks/useCreateProductCategory";
import { useProductCategories } from "../hooks/useProductCategories";

export function ProductCategoryListPage() {
  const [searchInput, setSearchInput] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [status, setStatus] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const pageSize = 20;

  const createMutation = useCreateProductCategory();

  const categoriesQuery = useProductCategories({
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
            Product categories
          </Typography>

          <Button
            variant="contained"
            onClick={() => {
              createMutation.reset();
              setCreateDialogOpen(true);
            }}
          >
            Create product category
          </Button>
        </Stack>

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
            <InputLabel id="product-category-status-label">Status</InputLabel>

            <Select
              labelId="product-category-status-label"
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

        {categoriesQuery.isFetching && !categoriesQuery.isPending && (
          <LinearProgress />
        )}

        {categoriesQuery.isPending && (
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <CircularProgress size={24} />
            <Typography>Loading product categories...</Typography>
          </Stack>
        )}

        {categoriesQuery.isError && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                disabled={categoriesQuery.isFetching}
                onClick={() => {
                  void categoriesQuery.refetch();
                }}
              >
                Retry
              </Button>
            }
          >
            {getErrorMessage(categoriesQuery.error)}
          </Alert>
        )}

        {categoriesQuery.data && categoriesQuery.data.items.length === 0 && (
          <Alert severity="info">No product categories found.</Alert>
        )}

        {categoriesQuery.data && categoriesQuery.data.items.length > 0 && (
          <Stack spacing={2}>
            <Typography>
              Total product categories: {categoriesQuery.data.totalCount}
            </Typography>

            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Code</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell>Description</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Created at</TableCell>
                    <TableCell>Last modified at</TableCell>
                    <TableCell align="right">Actions</TableCell>
                  </TableRow>
                </TableHead>

                <TableBody>
                  {categoriesQuery.data.items.map((category) => (
                    <TableRow key={category.productCategoryId} hover>
                      <TableCell>{category.code}</TableCell>
                      <TableCell>{category.name}</TableCell>
                      <TableCell>{category.description || "—"}</TableCell>
                      <TableCell>
                        <Chip
                          label={category.status}
                          color={
                            category.status === "Active" ? "success" : "default"
                          }
                          size="small"
                        />
                      </TableCell>
                      <TableCell>{formatDate(category.createdAt)}</TableCell>
                      <TableCell>
                        {formatDate(category.lastModifiedAt)}
                      </TableCell>
                      <TableCell align="right">
                        <Button
                          component={RouterLink}
                          to={`/product-categories/${category.productCategoryId}`}
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

            {categoriesQuery.data.totalPages > 0 && (
              <Stack direction="row" sx={{ justifyContent: "center" }}>
                <Pagination
                  page={pageNumber}
                  count={categoriesQuery.data.totalPages}
                  disabled={categoriesQuery.isFetching}
                  color="primary"
                  onChange={(_, selectedPage) => {
                    setPageNumber(selectedPage);
                  }}
                />
              </Stack>
            )}
          </Stack>
        )}
      </Stack>

      <CreateProductCategoryDialog
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

  return "Unable to load product categories.";
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
