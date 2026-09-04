import { reviewClient } from './httpClient';
export const reviewApi = {
  getForProduct: productId => reviewClient.get(`/api/reviews/products/${productId}`),
  create: (productId, payload) => reviewClient.post(`/api/reviews/products/${productId}`, payload),
  remove: reviewId => reviewClient.delete(`/api/reviews/${reviewId}`)
};
