import { catalogClient, inventoryClient, orderClient } from './httpClient';
export const adminApi = {
  createProduct: payload => catalogClient.post('/api/catalog/products', payload),
  updateProduct: (productId, payload) => catalogClient.put(`/api/catalog/products/${productId}`, payload),
  deleteProduct: productId => catalogClient.delete(`/api/catalog/products/${productId}`),
  getStock: productId => inventoryClient.get(`/api/inventory/stock/${productId}`),
  adjustStock: (productId, payload) => inventoryClient.put(`/api/inventory/stock/${productId}`, payload),
  listOrders: () => orderClient.get('/api/orders/'),
  updateOrderStatus: (orderId, payload) => orderClient.put(`/api/orders/${orderId}/status`, payload)
};
