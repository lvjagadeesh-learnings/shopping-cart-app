import { Link, useNavigate } from 'react-router-dom';
import { useCart, useRemoveCartItem, useUpdateCartItem } from '../hooks/useCart';

export function CartPage() {
  const { data: cart, isLoading, isError } = useCart();
  const updateItem = useUpdateCartItem();
  const removeItem = useRemoveCartItem();
  const navigate = useNavigate();

  if (isLoading) {
    return <p className="text-gray-500">Loading cart…</p>;
  }

  if (isError) {
    return <p className="text-red-500">Failed to load cart.</p>;
  }

  if (!cart || cart.items.length === 0) {
    return (
      <div className="py-16 text-center">
        <p className="mb-4 text-gray-500">Your cart is empty.</p>
        <Link to="/" className="bg-brand-500 hover:bg-brand-600 rounded px-4 py-2 text-white">
          Continue shopping
        </Link>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
      <div className="lg:col-span-2">
        <h1 className="mb-4 text-lg font-semibold text-gray-800">Shopping Cart</h1>
        <ul className="divide-y divide-gray-200 rounded border border-gray-200 bg-white">
          {cart.items.map((item) => (
            <li key={item.id} className="flex items-center gap-4 p-4">
              <img
                src={item.productImageUrl}
                alt={item.productName}
                className="h-16 w-16 rounded object-cover"
              />
              <div className="flex-1">
                <p className="text-sm text-gray-800">{item.productName}</p>
                <p className="text-brand-600 text-sm font-semibold">
                  ${item.unitPrice.toFixed(2)}
                </p>
              </div>
              <input
                type="number"
                min={1}
                value={item.quantity}
                onChange={(event) => {
                  const quantity = Math.max(1, Number(event.target.value));
                  updateItem.mutate({ productId: item.productId, quantity });
                }}
                className="w-16 rounded border border-gray-300 px-2 py-1 text-center"
              />
              <span className="w-20 text-right text-sm font-medium text-gray-800">
                ${item.lineTotal.toFixed(2)}
              </span>
              <button
                onClick={() => removeItem.mutate(item.productId)}
                className="text-sm text-red-500 hover:underline"
              >
                Remove
              </button>
            </li>
          ))}
        </ul>
      </div>

      <div className="h-fit rounded border border-gray-200 bg-white p-4">
        <h2 className="mb-4 text-lg font-semibold text-gray-800">Order Summary</h2>
        <div className="flex justify-between text-sm text-gray-600">
          <span>Items ({cart.totalItems})</span>
          <span>${cart.subtotal.toFixed(2)}</span>
        </div>
        <div className="mt-2 flex justify-between border-t border-gray-200 pt-2 text-base font-semibold text-gray-900">
          <span>Total</span>
          <span>${cart.subtotal.toFixed(2)}</span>
        </div>
        <button
          onClick={() => navigate('/checkout')}
          className="bg-brand-500 hover:bg-brand-600 mt-4 w-full rounded py-2 font-medium text-white"
        >
          Checkout
        </button>
      </div>
    </div>
  );
}
