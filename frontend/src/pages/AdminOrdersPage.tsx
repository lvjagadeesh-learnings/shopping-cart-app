import { useAdminOrders, useAdminUpdateOrderStatus } from '../hooks/useAdmin';
import type { OrderStatus } from '../api/types';

const ALL_STATUSES: OrderStatus[] = [
  'Placed',
  'Paid',
  'Preparing',
  'Shipped',
  'OutForDelivery',
  'Delivered',
  'Cancelled',
];

export function AdminOrdersPage() {
  const { data: orders, isLoading } = useAdminOrders();
  const updateStatus = useAdminUpdateOrderStatus();

  if (isLoading) {
    return <p className="text-gray-500">Loading orders…</p>;
  }

  return (
    <div>
      <h1 className="mb-6 text-lg font-semibold text-gray-800">Manage Orders</h1>
      <ul className="divide-y divide-gray-200 rounded border border-gray-200 bg-white">
        {orders?.map((order) => (
          <li key={order.id} className="flex items-center gap-4 p-4">
            <div className="flex-1">
              <p className="text-sm font-medium text-gray-800">Order #{order.id.slice(0, 8)}</p>
              <p className="text-xs text-gray-500">
                {new Date(order.createdAtUtc).toLocaleString()} · {order.totalItems} item(s) · $
                {order.totalAmount.toFixed(2)}
              </p>
            </div>
            <select
              value={order.status}
              onChange={(e) =>
                updateStatus.mutate({ orderId: order.id, payload: { status: e.target.value as OrderStatus } })
              }
              className="rounded border border-gray-300 px-3 py-1.5 text-sm"
            >
              {ALL_STATUSES.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </li>
        ))}
      </ul>
      {(!orders || orders.length === 0) && <p className="text-gray-500">No orders yet.</p>}
    </div>
  );
}
