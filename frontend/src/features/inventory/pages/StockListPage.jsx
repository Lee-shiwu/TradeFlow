import { useState } from "react";
import {
  Alert,
  Box,
  Button,
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
import { ApiError } from "../../../shared/api/ApiError";
import { useStock } from "../hooks/useStock";

export function StockListPage() {
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const stockQuery = useStock({ search, pageNumber, pageSize: 20 });

  return (
    <Box component="main" sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Typography component="h1" variant="h4">
          Inventory
        </Typography>
        <Typography color="text.secondary">
          Current quantity calculated from immutable stock transactions.
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
            placeholder="Search by SKU or product name"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
          />
          <Button type="submit" variant="contained">
            Search
          </Button>
        </Stack>
        {stockQuery.isFetching && !stockQuery.isPending && <LinearProgress />}
        {stockQuery.isPending && (
          <Stack direction="row" spacing={2} sx={{ alignItems: "center" }}>
            <CircularProgress size={24} />
            <Typography>Loading stock...</Typography>
          </Stack>
        )}
        {stockQuery.isError && (
          <Alert
            severity="error"
            action={
              <Button color="inherit" onClick={() => void stockQuery.refetch()}>
                Retry
              </Button>
            }
          >
            {stockQuery.error instanceof ApiError
              ? stockQuery.error.detail
              : "Unable to load stock."}
          </Alert>
        )}
        {stockQuery.data?.items.length === 0 && (
          <Alert severity="info">No stock found.</Alert>
        )}
        {stockQuery.data?.items.length > 0 && (
          <Stack spacing={2}>
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>SKU</TableCell>
                    <TableCell>Product</TableCell>
                    <TableCell align="right">Quantity on hand</TableCell>
                    <TableCell>Last movement</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {stockQuery.data.items.map((item) => (
                    <TableRow key={item.productId}>
                      <TableCell>{item.productSku}</TableCell>
                      <TableCell>{item.productName}</TableCell>
                      <TableCell align="right">{item.quantityOnHand}</TableCell>
                      <TableCell>
                        {new Date(item.lastMovementAt).toLocaleString()}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
            {stockQuery.data.totalPages > 0 && (
              <Pagination
                page={pageNumber}
                count={stockQuery.data.totalPages}
                disabled={stockQuery.isFetching}
                onChange={(_, page) => setPageNumber(page)}
              />
            )}
          </Stack>
        )}
      </Stack>
    </Box>
  );
}
