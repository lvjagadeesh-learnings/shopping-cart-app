import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { reviewApi } from '../api/reviewApi';
import type { CreateReviewRequest } from '../api/types';

export function useProductReviews(productId: string | undefined) {
  return useQuery({
    queryKey: ['reviews', productId],
    queryFn: () => reviewApi.getForProduct(productId as string),
    enabled: Boolean(productId),
  });
}

export function useCreateReview(productId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateReviewRequest) => reviewApi.create(productId, payload),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['reviews', productId] }),
  });
}

export function useDeleteReview(productId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (reviewId: string) => reviewApi.remove(reviewId),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['reviews', productId] }),
  });
}
