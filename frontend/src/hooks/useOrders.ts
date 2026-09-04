import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { orderApi } from '../api/orderApi';
import { useAuthStore } from '../store/authStore';

export function useCheckout() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: () => orderApi.checkout(),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cart'] });
      queryClient.invalidateQueries({ queryKey: ['orders'] });
    },
  });
}

export function useOrders() {
  const isAuthenticated = useAuthStore((state) => state.isAuthenticated);
  return useQuery({
    queryKey: ['orders'],
    queryFn: orderApi.listOrders,
    enabled: isAuthenticated,
  });
}

export function useOrder(orderId: string | undefined) {
  return useQuery({
    queryKey: ['order', orderId],
    queryFn: () => orderApi.getOrder(orderId as string),
    enabled: Boolean(orderId),
  });
}
