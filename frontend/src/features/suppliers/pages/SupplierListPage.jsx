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
import { CreateSupplierDialog } from "../components/CreateSupplierDialog";
import { useCreateSupplier } from "../hooks/useCreateSupplier";
import { useSuppliers } from "../hooks/useSuppliers";

export function SupplierListPage() {
  const [searchInput, setSearchInput] = useState("");
  const [appliedSearch, setAppliedSearch] = useState("");
  const [status, setStatus] = useState("Active");
  const [pageNumber, setPageNumber] = useState(1);
  const [createDialogOpen, setCreateDialogOpen] = useState(false);
  const pageSize = 20;
  const createMutation = useCreateSupplier();
  const suppliersQuery = useSuppliers({
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
            Suppliers
          </Typography>
          <Button
            variant="contained"
            onClick={() => {
              createMutation.reset();
              setCreateDialogOpen(true);
            }}
          >
            Create supplier
          </Button>
        </Stack>

        <Typography color="text.secondary">
          Manage the suppliers available when creating purchase orders.
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
            <InputLabel id="supplier-status-label">Status</InputLabel>
            <Select
              labelId="supplier-status-label"
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

        {suppliersQuery.isFetching && !suppliersQuery.isPending && (
          <LinearProgress />
        )}
        {suppliersQuery.isPending && (
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <CircularProgress size={24} />
            <Typography>Loading suppliers...</Typography>
          </Stack>
        )}
        {suppliersQuery.isError && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                size="small"
                disabled={suppliersQuery.isFetching}
                onClick={() => void suppliersQuery.refetch()}
              >
                Retry
              </Button>
            }
          >
            {getErrorMessage(suppliersQuery.error)}
          </Alert>
        )}
        {suppliersQuery.data && suppliersQuery.data.items.length === 0 && (
          <Alert severity="info">No suppliers found.</Alert>
        )}
        {suppliersQuery.data && suppliersQuery.data.items.length > 0 && (
          <Stack spacing={2}>
            <Typography>
              Total suppliers: {suppliersQuery.data.totalCount}
            </Typography>
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Code</TableCell>
                    <TableCell>Name</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell>Created at</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {suppliersQuery.data.items.map((supplier) => (
                    <TableRow key={supplier.supplierId} hover>
                      <TableCell>{supplier.code}</TableCell>
                      <TableCell>{supplier.name}</TableCell>
                      <TableCell>
                        <Chip
                          label={supplier.status}
                          color={
                            supplier.status === "Active" ? "success" : "default"
                          }
                          size="small"
                        />
                      </TableCell>
                      <TableCell>{formatDate(supplier.createdAt)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            {suppliersQuery.data.totalPages > 0 && (
              <Stack direction="row" sx={{ justifyContent: "center" }}>
                <Pagination
                  page={pageNumber}
                  count={suppliersQuery.data.totalPages}
                  disabled={suppliersQuery.isFetching}
                  color="primary"
                  onChange={(_, selectedPage) => setPageNumber(selectedPage)}
                />
              </Stack>
            )}
          </Stack>
        )}
      </Stack>

      <CreateSupplierDialog
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

  return "Unable to load suppliers.";
}

function formatDate(value) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}
