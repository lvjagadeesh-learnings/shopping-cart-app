import { catalogClient, inventoryClient, orderClient } from './httpClient';
import type {
  AdjustStockRequest,
  OrderResponse,
  OrderSummaryResponse,
  ProductSummaryResponse,
  StockLevelResponse,
  UpdateOrderStatusRequest,
  UpsertProductRequest,
} from './types';

export const adminApi = {
  createProduct: (payload: UpsertProductRequest) =>
    catalogClient.post<ProductSummaryResponse>('/api/catalog/products', payload),
  updateProduct: (productId: string, payload: UpsertProductRequest) =>
    catalogClient.put<ProductSummaryResponse>(`/api/catalog/products/${productId}`, payload),
  deleteProduct: (productId: string) =>
    catalogClient.delete<void>(`/api/catalog/products/${productId}`),
  getStock: (productId: string) =>
    inventoryClient.get<StockLevelResponse>(`/api/inventory/stock/${productId}`),
  adjustStock: (productId: string, payload: AdjustStockRequest) =>
    inventoryClient.put<StockLevelResponse>(`/api/inventory/stock/${productId}`, payload),
  listOrders: () => orderClient.get<OrderSummaryResponse[]>('/api/orders/'),
  updateOrderStatus: (orderId: string, payload: UpdateOrderStatusRequest) =>
    orderClient.put<OrderResponse>(`/api/orders/${orderId}/status`, payload),
};
