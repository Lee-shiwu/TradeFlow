import type { ReactNode } from 'react'
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
} from '@mui/material'
import {
  Link as RouterLink,
  useParams,
} from 'react-router-dom'
import { ApiError } from '../../../shared/api/ApiError'
import { useProduct } from '../hooks/useProduct'

export function ProductDetailsPage() {
  const { productId } = useParams<{
    productId: string
  }>()

  const productQuery = useProduct(productId)
  const product = productQuery.data

  const isNotFound =
    productQuery.error instanceof ApiError &&
    productQuery.error.status === 404

  return (
    <Box component="main" sx={{ p: 4 }}>
      <Stack spacing={3}>
        <Stack
          direction={{
            xs: 'column',
            sm: 'row',
          }}
          spacing={2}
          sx={{
            justifyContent: 'space-between',
            alignItems: {
              xs: 'flex-start',
              sm: 'center',
            },
          }}
        >
          <Typography component="h1" variant="h4">
            Product details
          </Typography>

          <Button
            component={RouterLink}
            to="/products"
            variant="outlined"
          >
            Back to products
          </Button>
        </Stack>

        {!productId && (
          <Alert severity="error">
            Product ID is missing.
          </Alert>
        )}

        {productId && productQuery.isPending && (
          <Stack
            direction="row"
            spacing={2}
            sx={{ alignItems: 'center' }}
          >
            <CircularProgress size={24} />

            <Typography>
              Loading product details...
            </Typography>
          </Stack>
        )}

        {productId &&
          productQuery.isFetching &&
          !productQuery.isPending && (
            <LinearProgress />
          )}

        {productId &&
          productQuery.isError &&
          isNotFound && (
            <Alert severity="info">
              The selected product does not exist
              or is not available in your organisation.
            </Alert>
          )}

        {productId &&
          productQuery.isError &&
          !isNotFound && (
            <Alert
              severity="error"
              action={
                <Button
                  color="inherit"
                  size="small"
                  disabled={productQuery.isFetching}
                  onClick={() => {
                    void productQuery.refetch()
                  }}
                >
                  Retry
                </Button>
              }
            >
              {getErrorMessage(productQuery.error)}
            </Alert>
          )}

        {product && !isNotFound && (
          <Paper sx={{ p: 3 }}>
            <Stack
              component="dl"
              spacing={2}
              sx={{ m: 0 }}
            >
              <ProductDetailField
                label="Product ID"
                value={product.productId}
              />

              <ProductDetailField
                label="SKU"
                value={product.sku}
              />

              <ProductDetailField
                label="Name"
                value={product.name}
              />

              <ProductDetailField
                label="Description"
                value={product.description || '—'}
              />

              <ProductDetailField
                label="Status"
                value={
                  <Chip
                    label={product.status}
                    color={
                      product.status === 'Active'
                        ? 'success'
                        : 'default'
                    }
                    size="small"
                  />
                }
              />

              <ProductDetailField
                label="Unit of measure ID"
                value={product.unitOfMeasureId}
              />

              <ProductDetailField
                label="Product category ID"
                value={product.productCategoryId ?? '—'}
              />

              <ProductDetailField
                label="Tax category ID"
                value={product.taxCategoryId}
              />

              <ProductDetailField
                label="Created at"
                value={formatDate(product.createdAt)}
              />

              <ProductDetailField
                label="Created by"
                value={product.createdBy}
              />

              <ProductDetailField
                label="Last modified at"
                value={formatDate(product.lastModifiedAt)}
              />

              <ProductDetailField
                label="Last modified by"
                value={product.lastModifiedBy ?? '—'}
              />
            </Stack>
          </Paper>
        )}
      </Stack>
    </Box>
  )
}

interface ProductDetailFieldProps {
  label: string
  value: ReactNode
}

function ProductDetailField({
  label,
  value,
}: ProductDetailFieldProps) {
  return (
    <Box>
      <Typography
        component="dt"
        variant="body2"
        color="text.secondary"
      >
        {label}
      </Typography>

      <Typography
        component="dd"
        variant="body1"
        sx={{
          m: 0,
          mt: 0.5,
          overflowWrap: 'anywhere',
          whiteSpace: 'pre-wrap',
        }}
      >
        {value}
      </Typography>
    </Box>
  )
}

function getErrorMessage(error: unknown): string {
  if (error instanceof ApiError) {
    return error.detail
  }

  return 'Unable to load product details.'
}

function formatDate(value: string | null): string {
  if (value === null) {
    return '—'
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(value))
}