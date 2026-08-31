import {
  beforeEach,
  describe,
  expect,
  it,
  vi,
} from 'vitest'
import { ApiError } from '../../../shared/api/ApiError'
import { getProductById } from './getProductById'
import type { ProductDetails } from './productDetailsTypes'

const apiRequestMock = vi.hoisted(() => vi.fn())

vi.mock('../../../shared/api/apiClient', () => ({
  apiRequest: apiRequestMock,
}))

const productId =
  '33333333-3333-3333-3333-333333333333'

const productDetails: ProductDetails = {
  productId,
  sku: 'CHAIR-001',
  name: 'Office Chair',
  description: 'Ergonomic office chair.',
  unitOfMeasureId:
    '44444444-4444-4444-4444-444444444444',
  productCategoryId: null,
  taxCategoryId:
    '55555555-5555-5555-5555-555555555555',
  status: 'Active',
  createdAt: '2026-08-27T00:00:00+00:00',
  createdBy:
    '66666666-6666-6666-6666-666666666666',
  lastModifiedAt: null,
  lastModifiedBy: null,
  rowVersion: 'AAAAAAAAB9E=',
}

describe('getProductById', () => {
  beforeEach(() => {
    apiRequestMock.mockReset()
  })

  it('requests the selected product using GET', async () => {
    apiRequestMock.mockResolvedValue(productDetails)

    await getProductById(productId)

    expect(apiRequestMock).toHaveBeenCalledOnce()

    expect(apiRequestMock).toHaveBeenCalledWith(
      `/api/v1/catalog/products/${productId}`,
      {
        method: 'GET',
      },
    )
  })

  it('returns the product details', async () => {
    apiRequestMock.mockResolvedValue(productDetails)

    const result = await getProductById(productId)

    expect(result).toEqual(productDetails)
  })

  it('propagates the error when the product is not found', async () => {
    const error = new ApiError(
      404,
      'PRODUCT_NOT_FOUND',
      'The selected product does not exist.',
      'not-found-trace-id',
    )

    apiRequestMock.mockRejectedValue(error)

    await expect(
      getProductById(productId),
    ).rejects.toBe(error)
  })

  it('propagates an unexpected server error', async () => {
    const error = new ApiError(
      500,
      'UNEXPECTED_ERROR',
      'An unexpected error occurred.',
      'server-error-trace-id',
    )

    apiRequestMock.mockRejectedValue(error)

    await expect(
      getProductById(productId),
    ).rejects.toBe(error)
  })
})
