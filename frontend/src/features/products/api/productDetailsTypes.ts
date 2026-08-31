import type { ProductStatus } from './productListTypes'

export interface ProductDetails {
  productId: string
  sku: string
  name: string
  description: string
  unitOfMeasureId: string
  productCategoryId: string | null
  taxCategoryId: string
  status: ProductStatus
  createdAt: string
  createdBy: string
  lastModifiedAt: string | null
  lastModifiedBy: string | null
  rowVersion: string
}