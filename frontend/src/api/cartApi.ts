import { cartClient } from './httpClient';
import type { AddCartItemRequest, CartResponse, UpdateCartItemRequest } from './types';

export const cartApi = {
  getCart: () => cartClient.get<CartResponse>('/api/cart/'),
  addItem: (payload: AddCartItemRequest) => cartClient.post<CartResponse>('/api/cart/items', payload),
  updateItem: (productId: string, payload: UpdateCartItemRequest) =>
    cartClient.put<CartResponse>(`/api/cart/items/${productId}`, payload),
  removeItem: (productId: string) => cartClient.delete<CartResponse>(`/api/cart/items/${productId}`),
  clearCart: () => cartClient.delete<void>('/api/cart/'),
};
