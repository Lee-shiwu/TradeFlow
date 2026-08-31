/** @typedef {"Active" | "Inactive"} ProductStatus */

/**
 * @typedef {Object} ListProductItem
 * @property {string} productId
 * @property {string} sku
 * @property {string} name
 * @property {string} unitOfMeasureId
 * @property {string | null} productCategoryId
 * @property {string} taxCategoryId
 * @property {ProductStatus} status
 * @property {string} createdAt
 * @property {string | null} lastModifiedAt
 */

/**
 * @typedef {Object} ListProductsParameters
 * @property {string} [search]
 * @property {ProductStatus} [status]
 * @property {number} pageNumber
 * @property {number} pageSize
 */

/**
 * @typedef {Object} ListProductsResponse
 * @property {ListProductItem[]} items
 * @property {number} pageSize
 * @property {number} pageNumber
 * @property {number} totalPages
 * @property {number} totalCount
 */

export {};
