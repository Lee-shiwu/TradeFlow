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
import type { SelectChangeEvent } from "@mui/material/Select";
import { ApiError } from "../../../shared/api/ApiError";
import type { ProductStatus } from "../api/productListTypes";
import { useProducts } from "../hooks/useProducts";

export function ProductListPage() {
  const [searchInput, setSearchInput] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [status, setStatus] = useState<ProductStatus | "">("");
  const [pageNumber, setPageNumber] = useState(1);

  const pageSize = 20;

  const productsQuery = useProducts({
    search: appliedSearch,
    status: status || undefined,
    pageNumber,
    pageSize,
  });

  function handleStatusChange(event: SelectChangeEvent) {
    setStatus(event.target.value as ProductStatus | "");
    setPageNumber(1);
  }

  return (
    <Box component="main" sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography component="h1" variant="h4">
          Products
        </Typography>

        <Stack
          component="form"
          direction={{
            xs: "column",
            sm: "row",
          }}
          spacing={2}
          onSubmit={(event) => {
            event.preventDefault();
            setAppliedSearch(searchInput.trim());
            setPageNumber(1);
          }}
        >
          <TextField
            label="Search"
            placeholder="Search by SKU or name"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
          />

          <FormControl sx={{ minWidth: 180 }}>
            <InputLabel id="product-status-label">Status</InputLabel>

            <Select
              labelId="product-status-label"
              label="Status"
              value={status}
              onChange={handleStatusChange}
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

        {productsQuery.isFetching && !productsQuery.isPending && (
          <LinearProgress />
        )}

        {productsQuery.isPending && (
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <CircularProgress size={24} />

            <Typography>Loading products...</Typography>
          </Stack>
        )}

        {productsQuery.isError && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                disabled={productsQuery.isFetching}
                onClick={() => {
                  void productsQuery.refetch();
                }}
              >
                Retry
              </Button>
            }
          >
            {getErrorMessage(productsQuery.error)}
          </Alert>
        )}

        {productsQuery.data && productsQuery.data.items.length === 0 && (
          <Alert severity="info">No products found.</Alert>
        )}

        {productsQuery.data && productsQuery.data.items.length > 0 && (
          <Stack spacing={2}>
            <Typography>
              Total products: {productsQuery.data.totalCount}
            </Typography>

            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>SKU</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Created at</TableCell>
                    <TableCell>Last modified at</TableCell>
                  </TableRow>
                </TableHead>

                <TableBody>
                  {productsQuery.data.items.map((product) => (
                    <TableRow key={product.productId} hover>
                      <TableCell>{product.sku}</TableCell>

                      <TableCell>{product.name}</TableCell>

                      <TableCell>
                        <Chip
                          label={product.status}
                          color={
                            product.status === "Active" ? "success" : "default"
                          }
                          size="small"
                        />
                      </TableCell>

                      <TableCell>{formatDate(product.createdAt)}</TableCell>

                      <TableCell>
                        {formatDate(product.lastModifiedAt)}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>

            {productsQuery.data.totalPages > 0 && (
              <Stack direction="row" sx={{ justifyContent: "center" }}>
                <Pagination
                  page={pageNumber}
                  count={productsQuery.data.totalPages}
                  disabled={productsQuery.isFetching}
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
    </Box>
  );
}

function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.detail;
  }

  return "Unable to load products.";
}

function formatDate(value: string | null): string {
  if (value === null) {
    return "—";
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}
