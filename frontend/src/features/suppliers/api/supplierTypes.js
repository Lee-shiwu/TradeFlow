/** @typedef {"Active" | "Inactive"} SupplierStatus */

/**
 * @typedef {Object} SupplierListItem
 * @property {string} supplierId
 * @property {string} code
 * @property {string} name
 * @property {SupplierStatus} status
 * @property {string} createdAt
 */

/**
 * @typedef {Object} ListSuppliersParameters
 * @property {string} [search]
 * @property {SupplierStatus} [status]
 * @property {number} pageNumber
 * @property {number} pageSize
 */

/**
 * @typedef {Object} ListSuppliersResponse
 * @property {SupplierListItem[]} items
 * @property {number} pageNumber
 * @property {number} pageSize
 * @property {number} totalCount
 * @property {number} totalPages
 */

/**
 * @typedef {Object} CreateSupplierRequest
 * @property {string} code
 * @property {string} name
 */

/**
 * @typedef {Object} CreateSupplierResponse
 * @property {string} supplierId
 * @property {string} code
 * @property {string} name
 * @property {SupplierStatus} status
 * @property {string} createdAt
 * @property {string} createdBy
 * @property {string} rowVersion
 */

export {};
