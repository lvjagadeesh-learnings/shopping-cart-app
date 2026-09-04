import { useQuery } from '@tanstack/react-query';
import { catalogApi } from '../api/catalogApi';
export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: catalogApi.listCategories,
    staleTime: 5 * 60 * 1000
  });
}
export function useProducts(query) {
  return useQuery({
    queryKey: ['products', query],
    queryFn: () => catalogApi.listProducts(query),
    staleTime: 60 * 1000
  });
}
export function useProduct(slug) {
  return useQuery({
    queryKey: ['product', slug],
    queryFn: () => catalogApi.getProductBySlug(slug),
    enabled: Boolean(slug)
  });
}
