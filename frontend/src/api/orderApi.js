import { orderClient } from './httpClient';
export const orderApi = {
  checkout: () => orderClient.post('/api/orders/checkout'),
  listOrders: () => orderClient.get('/api/orders/'),
  getOrder: orderId => orderClient.get(`/api/orders/${orderId}`),
  updateStatus: (orderId, payload) => orderClient.put(`/api/orders/${orderId}/status`, payload)
};
