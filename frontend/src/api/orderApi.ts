import { orderClient } from './httpClient';
import type { OrderResponse, OrderSummaryResponse, UpdateOrderStatusRequest } from './types';

export const orderApi = {
  checkout: () => orderClient.post<OrderResponse>('/api/orders/checkout'),
  listOrders: () => orderClient.get<OrderSummaryResponse[]>('/api/orders/'),
  getOrder: (orderId: string) => orderClient.get<OrderResponse>(`/api/orders/${orderId}`),
  updateStatus: (orderId: string, payload: UpdateOrderStatusRequest) =>
    orderClient.put<OrderResponse>(`/api/orders/${orderId}/status`, payload),
};
