import { useMutation, useQuery } from '@tanstack/react-query';
import { recommendationApi } from '../api/recommendationApi';
export function useRelatedProducts(productId) {
  return useQuery({
    queryKey: ['recommendations', 'related', productId],
    queryFn: () => recommendationApi.getRelated(productId),
    enabled: Boolean(productId),
    staleTime: 60 * 1000
  });
}
export function useTrendingProducts(take = 10) {
  return useQuery({
    queryKey: ['recommendations', 'trending', take],
    queryFn: () => recommendationApi.getTrending(take),
    staleTime: 60 * 1000
  });
}
export function useRecordProductView() {
  return useMutation({
    mutationFn: productId => recommendationApi.recordView(productId)
  });
}
