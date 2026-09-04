import { Link } from 'react-router-dom';
import { useOrders } from '../hooks/useOrders';

const STATUS_COLORS: Record<string, string> = {
  Placed: 'bg-gray-100 text-gray-700',
  Paid: 'bg-blue-100 text-blue-700',
  Preparing: 'bg-yellow-100 text-yellow-700',
  Shipped: 'bg-indigo-100 text-indigo-700',
  OutForDelivery: 'bg-purple-100 text-purple-700',
  Delivered: 'bg-green-100 text-green-700',
  Cancelled: 'bg-red-100 text-red-700',
};

export function OrderHistoryPage() {
  const { data: orders, isLoading, isError } = useOrders();

  if (isLoading) {
    return <p className="text-gray-500">Loading orders…</p>;
  }

  if (isError) {
    return <p className="text-red-500">Failed to load orders.</p>;
  }

  if (!orders || orders.length === 0) {
    return (
      <div className="py-16 text-center">
        <p className="mb-4 text-gray-500">You haven't placed any orders yet.</p>
        <Link to="/" className="bg-brand-500 hover:bg-brand-600 rounded px-4 py-2 text-white">
          Start Shopping
        </Link>
      </div>
    );
  }

  return (
    <div>
      <h1 className="mb-4 text-lg font-semibold text-gray-800">Order History</h1>
      <ul className="divide-y divide-gray-200 rounded border border-gray-200 bg-white">
        {orders.map((order) => (
          <li key={order.id} className="flex items-center justify-between p-4">
            <div>
              <p className="text-sm font-medium text-gray-800">Order #{order.id.slice(0, 8)}</p>
              <p className="text-xs text-gray-500">
                {new Date(order.createdAtUtc).toLocaleDateString()} · {order.totalItems} item(s)
              </p>
            </div>
            <span
              className={`rounded-full px-3 py-1 text-xs font-medium ${STATUS_COLORS[order.status] ?? 'bg-gray-100 text-gray-700'}`}
            >
              {order.status}
            </span>
            <span className="text-sm font-semibold text-gray-900">${order.totalAmount.toFixed(2)}</span>
            <Link to={`/orders/${order.id}`} className="text-brand-600 text-sm hover:underline">
              View
            </Link>
          </li>
        ))}
      </ul>
    </div>
  );
}
