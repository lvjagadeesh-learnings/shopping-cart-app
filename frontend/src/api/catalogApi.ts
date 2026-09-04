import { catalogClient } from './httpClient';
import type {
  CategoryResponse,
  PagedResult,
  ProductDetailResponse,
  ProductListQuery,
  ProductSummaryResponse,
} from './types';

function buildQuery(query: ProductListQuery): string {
  const params = new URLSearchParams();
  if (query.q) params.set('q', query.q);
  if (query.category) params.set('category', query.category);
  if (query.sort) params.set('sort', query.sort);
  if (query.page) params.set('page', String(query.page));
  if (query.pageSize) params.set('pageSize', String(query.pageSize));
  const qs = params.toString();
  return qs ? `?${qs}` : '';
}

export const catalogApi = {
  listCategories: () => catalogClient.get<CategoryResponse[]>('/api/catalog/categories'),
  listProducts: (query: ProductListQuery = {}) =>
    catalogClient.get<PagedResult<ProductSummaryResponse>>(
      `/api/catalog/products${buildQuery(query)}`,
    ),
  getProductBySlug: (slug: string) =>
    catalogClient.get<ProductDetailResponse>(`/api/catalog/products/${slug}`),
  getProductById: (id: string) =>
    catalogClient.get<ProductSummaryResponse>(`/api/catalog/products/id/${id}`),
};
