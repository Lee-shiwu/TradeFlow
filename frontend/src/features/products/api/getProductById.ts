import { apiRequest } from '../../../shared/api/apiClient'
import type { ProductDetails } from './productDetailsTypes'

export async function getProductById(
  productId: string,
): Promise<ProductDetails> {
  return apiRequest<ProductDetails>(
    `/api/v1/catalog/products/${encodeURIComponent(productId)}`,
    {
      method: 'GET',
    },
  )
}