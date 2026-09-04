import { reviewClient } from './httpClient';
import type { CreateReviewRequest, ProductReviewSummaryResponse, ReviewResponse } from './types';

export const reviewApi = {
  getForProduct: (productId: string) =>
    reviewClient.get<ProductReviewSummaryResponse>(`/api/reviews/products/${productId}`),
  create: (productId: string, payload: CreateReviewRequest) =>
    reviewClient.post<ReviewResponse>(`/api/reviews/products/${productId}`, payload),
  remove: (reviewId: string) => reviewClient.delete<void>(`/api/reviews/${reviewId}`),
};
