import { useParams } from 'react-router-dom';
import { useOrder } from '../hooks/useOrders';
const FULFILLMENT_STEPS = ['Placed', 'Paid', 'Preparing', 'Shipped', 'OutForDelivery', 'Delivered'];
export function OrderDetailPage() {
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
  const isCancelled = order.status === 'Cancelled';
  const currentStepIndex = FULFILLMENT_STEPS.indexOf(order.status);
  return <div>
      <h1 className="mb-1 text-lg font-semibold text-gray-800">Order #{order.id.slice(0, 8)}</h1>
      <p className="mb-6 text-sm text-gray-500">
        Placed on {new Date(order.createdAtUtc).toLocaleString()}
      </p>

      {isCancelled ? <div className="mb-6 rounded border border-red-200 bg-red-50 p-4 text-sm text-red-700">
          This order has been cancelled.
        </div> : <ol className="mb-8 flex flex-wrap items-center gap-2">
          {FULFILLMENT_STEPS.map((step, index) => <li key={step} className="flex items-center gap-2">
              <span className={`flex h-8 w-8 items-center justify-center rounded-full text-xs font-bold ${index <= currentStepIndex ? 'bg-brand-500 text-white' : 'bg-gray-100 text-gray-400'}`}>
                {index + 1}
              </span>
              <span className={`text-sm ${index <= currentStepIndex ? 'text-gray-800' : 'text-gray-400'}`}>
                {step}
              </span>
              {index < FULFILLMENT_STEPS.length - 1 && <span className="text-gray-300">—</span>}
            </li>)}
        </ol>}

      <ul className="divide-y divide-gray-200 rounded border border-gray-200 bg-white">
        {order.items.map(item => <li key={item.id} className="flex items-center gap-4 p-4">
            <img src={item.productImageUrl} alt={item.productName} className="h-14 w-14 rounded object-cover" />
            <div className="flex-1">
              <p className="text-sm text-gray-800">{item.productName}</p>
              <p className="text-xs text-gray-500">
                ${item.unitPrice.toFixed(2)} × {item.quantity}
              </p>
            </div>
            <span className="text-sm font-medium text-gray-800">${item.lineTotal.toFixed(2)}</span>
          </li>)}
      </ul>

      <div className="mt-4 flex justify-end text-base font-semibold text-gray-900">
        Total: ${order.totalAmount.toFixed(2)}
      </div>
    </div>;
}
