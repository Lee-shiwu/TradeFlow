export type ProductStatus = "Active" | "Inactive";

export interface ListProductItem {
  productId: string;
  sku: string;
  name: string;
  unitOfMeasureId: string;
  productCategoryId: string | null;
  taxCategoryId: string;
  status: ProductStatus;
  createdAt: string;
  lastModifiedAt: string | null;
}

export interface ListProductsParameters {
  search?: string;
  status?: ProductStatus;
  pageNumber: number;
  pageSize: number;
}

export interface ListProductsResponse {
  items: ListProductItem[];
  pageSize: number;
  pageNumber: number;
  totalPages: number;
  totalCount: number;
}
