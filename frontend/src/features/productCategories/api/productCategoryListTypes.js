/** @typedef {"Active" | "Inactive"} ProductCategoryStatus */

/**
 * @typedef {Object} ListProductCategoryItem
 * @property {string} productCategoryId
 * @property {string} code
 * @property {string} name
 * @property {string} description
 * @property {ProductCategoryStatus} status
 * @property {string} createdAt
 * @property {string | null} lastModifiedAt
 */

/**
 * @typedef {Object} ListProductCategoriesParameters
 * @property {string} [search]
 * @property {ProductCategoryStatus} [status]
 * @property {number} pageNumber
 * @property {number} pageSize
 */

/**
 * @typedef {Object} ListProductCategoriesResponse
 * @property {ListProductCategoryItem[]} items
 * @property {number} pageNumber
 * @property {number} pageSize
 * @property {number} totalCount
 * @property {number} totalPages
 */

export {};
