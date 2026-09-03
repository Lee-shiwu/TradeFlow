/**
 * 修改商品详情时发送给后端的数据。
 * @typedef {object} UpdateProductDetailsRequest
 * @property {string} name
 * @property {string | null} description
 * @property {string |null} productCategoryId
 * @property {string} taxCategoryId
 * @property {string} rowVersion
 *
 */

/**
 * 修改商品详情成功后后端返回的数据。
 * @typedef {object} UpdateProductDetailsResponse
 * @property {string} productId
 * @property {string} name
 * @property {string} description
 * @property {string |null} productCategoryId
 * @property {string} taxCategoryId
 * @property {string | null} lastModifiedAt
 * @property {string | null} lastModifiedBy
 * @property {string} rowVersion
 *
 *
 */
export {};
