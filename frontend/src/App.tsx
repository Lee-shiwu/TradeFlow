import { Box, Button, Container, Stack, Typography } from "@mui/material";
import { Navigate, NavLink, Route, Routes } from "react-router-dom";
import { ProductListPage } from "./features/products/pages/ProductListPage";
import { SystemStatusPage } from "./features/system/pages/SystemStatusPage";
import { ProductDetailsPage } from "./features/products/pages/ProductDetailsPage";

export default function App() {
  return (
    <Box
      sx={{
        minHeight: "100vh",
        py: {
          xs: 4,
          md: 8,
        },
      }}
    >
      <Container maxWidth="lg">
        <Stack spacing={4}>
          <Box component="header">
            <Typography
              component="h1"
              variant="h3"
              gutterBottom
              sx={{ fontWeight: 700 }}
            >
              TradeFlow
            </Typography>

            <Typography color="text.secondary" sx={{ mb: 3 }}>
              Enterprise purchasing, inventory and sales platform.
            </Typography>

            <Stack component="nav" direction="row" spacing={1}>
              <Button component={NavLink} to="/" variant="text">
                Home
              </Button>

              <Button component={NavLink} to="/products" variant="text">
                Products
              </Button>
            </Stack>
          </Box>

          <Routes>
            <Route path="/" element={<SystemStatusPage />} />

            <Route path="/products" element={<ProductListPage />} />

            <Route
              path="/products/:productId"
              element={<ProductDetailsPage />}
            />

            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Stack>
      </Container>
    </Box>
  );
}
