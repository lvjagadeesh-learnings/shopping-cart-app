import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { adminApi } from '../api/adminApi';
export function useAdminOrders() {
  return useQuery({
    queryKey: ['admin', 'orders'],
    queryFn: adminApi.listOrders
  });
}
export function useAdminUpdateOrderStatus() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      orderId,
      payload
    }) => adminApi.updateOrderStatus(orderId, payload),
    onSuccess: () => queryClient.invalidateQueries({
      queryKey: ['admin', 'orders']
    })
  });
}
export function useAdminCreateProduct() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: payload => adminApi.createProduct(payload),
    onSuccess: () => queryClient.invalidateQueries({
      queryKey: ['products']
    })
  });
}
export function useAdminUpdateProduct() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      productId,
      payload
    }) => adminApi.updateProduct(productId, payload),
    onSuccess: () => queryClient.invalidateQueries({
      queryKey: ['products']
    })
  });
}
export function useAdminDeleteProduct() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: productId => adminApi.deleteProduct(productId),
    onSuccess: () => queryClient.invalidateQueries({
      queryKey: ['products']
    })
  });
}
export function useAdminStockLevel(productId) {
  return useQuery({
    queryKey: ['admin', 'stock', productId],
    queryFn: () => adminApi.getStock(productId),
    enabled: Boolean(productId)
  });
}
export function useAdminAdjustStock() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({
      productId,
      payload
    }) => adminApi.adjustStock(productId, payload),
    onSuccess: (_data, variables) => queryClient.invalidateQueries({
      queryKey: ['admin', 'stock', variables.productId]
    })
  });
}
