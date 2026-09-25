/** @typedef {"StandardRated" | "ZeroRated" | "Exempt"} TaxTreatment */
/** @typedef {"Active" | "Inactive"} TaxCategoryStatus */

/**
 * @typedef {Object} CreateTaxCategoryRequest
 * @property {string} code
 * @property {string} name
 * @property {string | null} description
 * @property {number} rate
 * @property {TaxTreatment} treatment
 * @property {string} effectiveFrom
 * @property {string | null} effectiveTo
 */

/**
 * @typedef {Object} CreateTaxCategoryResponse
 * @property {string} taxCategoryId
 * @property {string} code
 * @property {string} name
 * @property {string} description
 * @property {number} rate
 * @property {TaxTreatment} treatment
 * @property {TaxCategoryStatus} status
 * @property {string} effectiveFrom
 * @property {string | null} effectiveTo
 * @property {string} rowVersion
 */

export {};
