import { recommendationClient } from './httpClient';
export const recommendationApi = {
  getRelated: productId => recommendationClient.get(`/api/recommendations/related/${productId}`),
  getTrending: (take = 10) => recommendationClient.get(`/api/recommendations/trending?take=${take}`),
  recordView: productId => recommendationClient.post('/api/recommendations/views', {
    productId
  })
};
