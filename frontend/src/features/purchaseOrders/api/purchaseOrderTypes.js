/** @typedef {"Draft" | "Confirmed"} PurchaseOrderStatus */

/**
 * @typedef {Object} CreatePurchaseOrderLineRequest
 * @property {string} productId
 * @property {number} quantity
 * @property {number} unitPrice
 */

/**
 * @typedef {Object} CreatePurchaseOrderRequest
 * @property {string} supplierId
 * @property {string} reference
 * @property {CreatePurchaseOrderLineRequest[]} lines
 */

/**
 * @typedef {Object} PurchaseOrderListItem
 * @property {string} purchaseOrderId
 * @property {string} supplierId
 * @property {string} supplierCode
 * @property {string} supplierName
 * @property {string} reference
 * @property {PurchaseOrderStatus} status
 * @property {number} lineCount
 * @property {number} totalAmount
 * @property {string} createdAt
 * @property {string} rowVersion
 */

/**
 * @typedef {Object} ListPurchaseOrdersResponse
 * @property {PurchaseOrderListItem[]} items
 * @property {number} pageNumber
 * @property {number} pageSize
 * @property {number} totalCount
 * @property {number} totalPages
 */

export {};
