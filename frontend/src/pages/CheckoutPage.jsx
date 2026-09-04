import { useNavigate } from 'react-router-dom';
import { useCart } from '../hooks/useCart';
import { useCheckout } from '../hooks/useOrders';
export function CheckoutPage() {
  const {
    data: cart,
    isLoading
  } = useCart();
  const checkout = useCheckout();
  const navigate = useNavigate();
  function handlePlaceOrder() {
    checkout.mutate(undefined, {
      onSuccess: order => navigate(`/orders/${order.id}/confirmation`)
    });
  }
  if (isLoading) {
    return <p className="text-gray-500">Loading checkout…</p>;
  }
  if (!cart || cart.items.length === 0) {
    return <p className="text-gray-500">Your cart is empty. Add items before checking out.</p>;
  }
  return <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
      <div className="lg:col-span-2 space-y-6">
        <section className="rounded border border-gray-200 bg-white p-4">
          <h2 className="mb-3 text-lg font-semibold text-gray-800">Shipping Address</h2>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <input className="rounded border border-gray-300 px-3 py-2 text-sm" placeholder="Full name" />
            <input className="rounded border border-gray-300 px-3 py-2 text-sm" placeholder="Phone number" />
            <input className="col-span-full rounded border border-gray-300 px-3 py-2 text-sm" placeholder="Street address" />
            <input className="rounded border border-gray-300 px-3 py-2 text-sm" placeholder="City" />
            <input className="rounded border border-gray-300 px-3 py-2 text-sm" placeholder="Postal code" />
          </div>
        </section>

        <section className="rounded border border-gray-200 bg-white p-4">
          <h2 className="mb-3 text-lg font-semibold text-gray-800">Payment Method (Simulated)</h2>
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <input className="col-span-full rounded border border-gray-300 px-3 py-2 text-sm" placeholder="Card number (any digits)" defaultValue="4242 4242 4242 4242" />
            <input className="rounded border border-gray-300 px-3 py-2 text-sm" placeholder="MM/YY" defaultValue="12/28" />
            <input className="rounded border border-gray-300 px-3 py-2 text-sm" placeholder="CVC" defaultValue="123" />
          </div>
          <p className="mt-2 text-xs text-gray-400">
            This is a simulated payment gateway — no real card processing occurs.
          </p>
        </section>
      </div>

      <div className="h-fit rounded border border-gray-200 bg-white p-4">
        <h2 className="mb-4 text-lg font-semibold text-gray-800">Order Summary</h2>
        <ul className="mb-3 space-y-2">
          {cart.items.map(item => <li key={item.id} className="flex justify-between text-sm text-gray-600">
              <span>
                {item.productName} × {item.quantity}
              </span>
              <span>${item.lineTotal.toFixed(2)}</span>
            </li>)}
        </ul>
        <div className="flex justify-between border-t border-gray-200 pt-2 text-base font-semibold text-gray-900">
          <span>Total</span>
          <span>${cart.subtotal.toFixed(2)}</span>
        </div>
        <button onClick={handlePlaceOrder} disabled={checkout.isPending} className="bg-brand-500 hover:bg-brand-600 mt-4 w-full rounded py-2 font-medium text-white disabled:opacity-50">
          {checkout.isPending ? 'Placing order…' : 'Place Order'}
        </button>
        {checkout.isError && <p className="mt-2 text-sm text-red-500">
            Payment was declined or an error occurred. Please try again.
          </p>}
      </div>
    </div>;
}
