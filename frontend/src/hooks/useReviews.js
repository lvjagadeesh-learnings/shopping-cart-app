import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { reviewApi } from '../api/reviewApi';
export function useProductReviews(productId) {
  return useQuery({
    queryKey: ['reviews', productId],
    queryFn: () => reviewApi.getForProduct(productId),
    enabled: Boolean(productId)
  });
}
export function useCreateReview(productId) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: payload => reviewApi.create(productId, payload),
    onSuccess: () => queryClient.invalidateQueries({
      queryKey: ['reviews', productId]
    })
  });
}
export function useDeleteReview(productId) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: reviewId => reviewApi.remove(reviewId),
    onSuccess: () => queryClient.invalidateQueries({
      queryKey: ['reviews', productId]
    })
  });
}
