/** @typedef {"Active" | "Inactive"} TaxCategoryStatus */
/** @typedef {"StandardRated" | "ZeroRated" | "Exempt"} TaxTreatment */

/**
 * @typedef {Object} TaxCategoryDetails
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
