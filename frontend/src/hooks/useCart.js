import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { cartApi } from '../api/cartApi';
import { useAuthStore } from '../store/authStore';
const CART_QUERY_KEY = ['cart'];
export function useCart() {
  const isAuthenticated = useAuthStore(state => state.isAuthenticated);
  return useQuery({
    queryKey: CART_QUERY_KEY,
    queryFn: cartApi.getCart,
    enabled: isAuthenticated
  });
}
export function useAddToCart() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      productId,
      quantity
    }) => cartApi.addItem({
      productId,
      quantity
    }),
    onSuccess: data => {
      queryClient.setQueryData(CART_QUERY_KEY, data);
    }
  });
}
export function useUpdateCartItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      productId,
      quantity
    }) => cartApi.updateItem(productId, {
      quantity
    }),
    onSuccess: data => {
      queryClient.setQueryData(CART_QUERY_KEY, data);
    }
  });
}
export function useRemoveCartItem() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: productId => cartApi.removeItem(productId),
    onSuccess: data => {
      queryClient.setQueryData(CART_QUERY_KEY, data);
    }
  });
}
