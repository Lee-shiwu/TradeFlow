import { apiRequest } from "../../../shared/api/apiClient";
import type {
  ListProductsParameters,
  ListProductsResponse,
} from "./productListTypes";

export async function listProducts(
  parameters: ListProductsParameters,
): Promise<ListProductsResponse> {
  const searchParameter = new URLSearchParams();

  const nomalizedSearch = parameters.search?.trim();

  if (nomalizedSearch) {
    searchParameter.set("search", nomalizedSearch);
  }

  if (parameters.status) {
    searchParameter.set("status", parameters.status);
  }

  searchParameter.set("pageNumber", parameters.pageNumber.toString());

  searchParameter.set("pageSize", parameters.pageSize.toString());

  const queryString = searchParameter.toString();

  return apiRequest<ListProductsResponse>(
    `/api/v1/catalog/products?${queryString}`,
    {
      method: "GET",
    },
  );
}
