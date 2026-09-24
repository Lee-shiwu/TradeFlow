import { Box, Button, Container, Stack, Typography } from "@mui/material";
import { Navigate, NavLink, Route, Routes } from "react-router-dom";
import { ProductCategoryDetailsPage } from "./features/productCategories/pages/ProductCategoryDetailsPage";
import { ProductCategoryListPage } from "./features/productCategories/pages/ProductCategoryListPage";
import { ProductDetailsPage } from "./features/products/pages/ProductDetailsPage";
import { ProductListPage } from "./features/products/pages/ProductListPage";
import { SystemStatusPage } from "./features/system/pages/SystemStatusPage";
import { TaxCategoryListPage } from "./features/taxCategories/pages/TaxCategoryListPage";
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

              <Button
                component={NavLink}
                to="/product-categories"
                variant="text"
              >
                Product categories
              </Button>

              <Button component={NavLink} to="/tax-categories" variant="text">
                Tax categories
              </Button>
            </Stack>
          </Box>

          <Routes>
            <Route path="/" element={<SystemStatusPage />} />

            <Route path="/products" element={<ProductListPage />} />

            <Route
              path="/product-categories"
              element={<ProductCategoryListPage />}
            />

            <Route
              path="/product-categories/:productCategoryId"
              element={<ProductCategoryDetailsPage />}
            />

            <Route path="/tax-categories" element={<TaxCategoryListPage />} />

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
