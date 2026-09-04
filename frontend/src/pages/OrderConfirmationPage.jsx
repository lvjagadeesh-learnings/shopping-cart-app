import { Link, useParams } from 'react-router-dom';
import { useOrder } from '../hooks/useOrders';
export function OrderConfirmationPage() {
  const {
    orderId
  } = useParams();
  const {
    data: order,
    isLoading,
    isError
  } = useOrder(orderId);
  if (isLoading) {
    return <p className="text-gray-500">Loading order…</p>;
  }
  if (isError || !order) {
    return <p className="text-red-500">Order not found.</p>;
  }
  return <div className="mx-auto max-w-lg py-8 text-center">
      <p className="text-4xl">🎉</p>
      <h1 className="mt-2 text-xl font-semibold text-gray-900">Order Placed!</h1>
      <p className="mt-1 text-sm text-gray-500">
        Order #{order.id.slice(0, 8)} — Total ${order.totalAmount.toFixed(2)}
      </p>

      <ul className="mt-6 divide-y divide-gray-200 rounded border border-gray-200 bg-white text-left">
        {order.items.map(item => <li key={item.id} className="flex items-center gap-4 p-4">
            <img src={item.productImageUrl} alt={item.productName} className="h-14 w-14 rounded object-cover" />
            <div className="flex-1">
              <p className="text-sm text-gray-800">{item.productName}</p>
              <p className="text-xs text-gray-500">Qty {item.quantity}</p>
            </div>
            <span className="text-sm font-medium text-gray-800">${item.lineTotal.toFixed(2)}</span>
          </li>)}
      </ul>

      <div className="mt-6 flex justify-center gap-4">
        <Link to={`/orders/${order.id}`} className="bg-brand-500 hover:bg-brand-600 rounded px-4 py-2 text-white">
          Track Order
        </Link>
        <Link to="/" className="rounded border border-gray-300 px-4 py-2 text-gray-700 hover:bg-gray-50">
          Continue Shopping
        </Link>
      </div>
    </div>;
}
