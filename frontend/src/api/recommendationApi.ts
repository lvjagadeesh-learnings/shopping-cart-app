import { recommendationClient } from './httpClient';
import type { RecommendedProductResponse } from './types';

export const recommendationApi = {
  getRelated: (productId: string) =>
    recommendationClient.get<RecommendedProductResponse[]>(`/api/recommendations/related/${productId}`),
  getTrending: (take = 10) =>
    recommendationClient.get<RecommendedProductResponse[]>(`/api/recommendations/trending?take=${take}`),
  recordView: (productId: string) =>
    recommendationClient.post<void>('/api/recommendations/views', { productId }),
};
