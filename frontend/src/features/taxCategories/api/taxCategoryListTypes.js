/** @typedef {"Active" | "Inactive"} TaxCategoryStatus */
/** @typedef {"StandardRated" | "ZeroRated" | "Exempt"} TaxTreatment */

/**
 * @typedef {Object} ListTaxCategoryItem
 * @property {string} taxCategoryId
 * @property {string} code
 * @property {string} name
 * @property {string} description
 * @property {number} rate
 * @property {TaxTreatment} treatment
 * @property {TaxCategoryStatus} status
 * @property {string} effectiveFrom
 * @property {string | null} effectiveTo
 */

/**
 * @typedef {Object} ListTaxCategoriesParameters
 * @property {string} [search]
 * @property {TaxCategoryStatus} [status]
 * @property {number} pageNumber
 * @property {number} pageSize
 */

/**
 * @typedef {Object} ListTaxCategoriesResponse
 * @property {ListTaxCategoryItem[]} items
 * @property {number} pageNumber
 * @property {number} pageSize
 * @property {number} totalCount
 * @property {number} totalPages
 */

export {};
