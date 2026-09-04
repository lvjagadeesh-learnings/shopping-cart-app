import { useQuery } from '@tanstack/react-query';
import { catalogApi } from '../api/catalogApi';
import type { ProductListQuery } from '../api/types';

export function useCategories() {
  return useQuery({
    queryKey: ['categories'],
    queryFn: catalogApi.listCategories,
    staleTime: 5 * 60 * 1000,
  });
}

export function useProducts(query: ProductListQuery) {
  return useQuery({
    queryKey: ['products', query],
    queryFn: () => catalogApi.listProducts(query),
    staleTime: 60 * 1000,
  });
}

export function useProduct(slug: string | undefined) {
  return useQuery({
    queryKey: ['product', slug],
    queryFn: () => catalogApi.getProductBySlug(slug as string),
    enabled: Boolean(slug),
  });
}
