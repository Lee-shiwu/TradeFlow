import { useState } from "react";
import {
  Alert,
  Box,
  Button,
  Chip,
  CircularProgress,
  LinearProgress,
  Pagination,
  Paper,
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
import { useProducts } from "../../products/hooks/useProducts";
import { useSuppliers } from "../../suppliers/hooks/useSuppliers";
import { ApiError } from "../../../shared/api/ApiError";
import { CreatePurchaseOrderDialog } from "../components/CreatePurchaseOrderDialog";
import { useCreatePurchaseOrder } from "../hooks/useCreatePurchaseOrder";
import { useConfirmPurchaseOrder } from "../hooks/useConfirmPurchaseOrder";
import { useReceivePurchaseOrder } from "../hooks/useReceivePurchaseOrder";
import { usePurchaseOrders } from "../hooks/usePurchaseOrders";

export function PurchaseOrderListPage() {
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [dialogOpen, setDialogOpen] = useState(false);
  const pageSize = 20;
  const ordersQuery = usePurchaseOrders({
    search,
    status: undefined,
    pageNumber,
    pageSize,
  });
  const suppliersQuery = useSuppliers({
    status: "Active",
    pageNumber: 1,
    pageSize: 100,
  });
  const productsQuery = useProducts({
    status: "Active",
    pageNumber: 1,
    pageSize: 100,
  });
  const createMutation = useCreatePurchaseOrder();
  const confirmMutation = useConfirmPurchaseOrder();
  const receiveMutation = useReceivePurchaseOrder();
  const referenceDataReady = Boolean(suppliersQuery.data && productsQuery.data);

  return (
    <Box component="main" sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack
          direction={{ xs: "column", sm: "row" }}
          spacing={2}
          sx={{ justifyContent: "space-between" }}
        >
          <Typography component="h1" variant="h4">
            Purchase orders
          </Typography>
          <Button
            variant="contained"
            disabled={!referenceDataReady}
            onClick={() => {
              createMutation.reset();
              setDialogOpen(true);
            }}
          >
            Create purchase order
          </Button>
        </Stack>
        <Typography color="text.secondary">
          Create draft purchase orders for active suppliers and products.
        </Typography>
        <Stack
          component="form"
          direction={{ xs: "column", sm: "row" }}
          spacing={2}
          onSubmit={(event) => {
            event.preventDefault();
            setSearch(searchInput.trim());
            setPageNumber(1);
          }}
        >
          <TextField
            label="Search"
            placeholder="Search by order reference"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
          />
          <Button type="submit" variant="contained">
            Search
          </Button>
        </Stack>

        {ordersQuery.isFetching && !ordersQuery.isPending && <LinearProgress />}
        {ordersQuery.isPending && (
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <CircularProgress size={24} />
            <Typography>Loading purchase orders...</Typography>
          </Stack>
        )}
        {(suppliersQuery.isError || productsQuery.isError) && (
          <Alert severity="error">
            Unable to load suppliers or products for order creation.
          </Alert>
        )}
        {ordersQuery.isError && (
          <Alert
            severity="error"
            action={
              <Button
                color="inherit"
                disabled={ordersQuery.isFetching}
                onClick={() => void ordersQuery.refetch()}
              >
                Retry
              </Button>
            }
          >
            {getErrorMessage(ordersQuery.error)}
          </Alert>
        )}
        {confirmMutation.isError && (
          <Alert severity="error">
            {getErrorMessage(confirmMutation.error)}
          </Alert>
        )}
        {receiveMutation.isError && (
          <Alert severity="error">
            {getErrorMessage(receiveMutation.error)}
          </Alert>
        )}
        {ordersQuery.data?.items.length === 0 && (
          <Alert severity="info">No purchase orders found.</Alert>
        )}
        {ordersQuery.data?.items.length > 0 && (
          <Stack spacing={2}>
            <Typography>
              Total purchase orders: {ordersQuery.data.totalCount}
            </Typography>
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Reference</TableCell>
                    <TableCell>Supplier</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="right">Lines</TableCell>
                    <TableCell align="right">Total</TableCell>
                    <TableCell>Created at</TableCell>
                    <TableCell>Actions</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {ordersQuery.data.items.map((order) => (
                    <TableRow key={order.purchaseOrderId} hover>
                      <TableCell>{order.reference}</TableCell>
                      <TableCell>
                        {order.supplierCode} — {order.supplierName}
                      </TableCell>
                      <TableCell>
                        <Chip label={order.status} size="small" />
                      </TableCell>
                      <TableCell align="right">{order.lineCount}</TableCell>
                      <TableCell align="right">
                        {formatMoney(order.totalAmount)}
                      </TableCell>
                      <TableCell>{formatDate(order.createdAt)}</TableCell>
                      <TableCell>
                        {order.status === "Draft" && (
                          <Button
                            size="small"
                            disabled={confirmMutation.isPending}
                            onClick={() =>
                              confirmMutation.mutate({
                                purchaseOrderId: order.purchaseOrderId,
                                rowVersion: order.rowVersion,
                              })
                            }
                          >
                            {confirmMutation.isPending
                              ? "Confirming..."
                              : "Confirm"}
                          </Button>
                        )}
                        {order.status === "Confirmed" && (
                          <Button
                            size="small"
                            disabled={receiveMutation.isPending}
                            onClick={() =>
                              receiveMutation.mutate({
                                purchaseOrderId: order.purchaseOrderId,
                                rowVersion: order.rowVersion,
                              })
                            }
                          >
                            {receiveMutation.isPending
                              ? "Receiving..."
                              : "Receive"}
                          </Button>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            {ordersQuery.data.totalPages > 0 && (
              <Pagination
                page={pageNumber}
                count={ordersQuery.data.totalPages}
                disabled={ordersQuery.isFetching}
                onChange={(_, page) => setPageNumber(page)}
              />
            )}
          </Stack>
        )}
      </Stack>

      <CreatePurchaseOrderDialog
        open={dialogOpen}
        suppliers={suppliersQuery.data?.items ?? []}
        products={productsQuery.data?.items ?? []}
        isPending={createMutation.isPending}
        error={createMutation.error}
        onSubmit={(request) => createMutation.mutateAsync(request)}
        onClose={() => {
          if (!createMutation.isPending) {
            createMutation.reset();
            setDialogOpen(false);
          }
        }}
      />
    </Box>
  );
}

function getErrorMessage(error) {
  return error instanceof ApiError
    ? error.detail
    : "Unable to load purchase orders.";
}

function formatMoney(value) {
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency: "NZD",
  }).format(value);
}

function formatDate(value) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(new Date(value));
}
