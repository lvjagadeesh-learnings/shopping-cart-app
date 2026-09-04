import { catalogClient } from './httpClient';
function buildQuery(query) {
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
  listCategories: () => catalogClient.get('/api/catalog/categories'),
  listProducts: (query = {}) => catalogClient.get(`/api/catalog/products${buildQuery(query)}`),
  getProductBySlug: slug => catalogClient.get(`/api/catalog/products/${slug}`),
  getProductById: id => catalogClient.get(`/api/catalog/products/id/${id}`)
};
