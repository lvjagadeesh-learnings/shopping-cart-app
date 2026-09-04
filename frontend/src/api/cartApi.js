import { cartClient } from './httpClient';
export const cartApi = {
  getCart: () => cartClient.get('/api/cart/'),
  addItem: payload => cartClient.post('/api/cart/items', payload),
  updateItem: (productId, payload) => cartClient.put(`/api/cart/items/${productId}`, payload),
  removeItem: productId => cartClient.delete(`/api/cart/items/${productId}`),
  clearCart: () => cartClient.delete('/api/cart/')
};
