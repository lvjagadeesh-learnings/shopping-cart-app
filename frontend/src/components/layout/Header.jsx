import { Link, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { useAuthStore } from '../../store/authStore';
import { useCart } from '../../hooks/useCart';
import { useLogout } from '../../hooks/useAuth';
import { NotificationsBell } from '../NotificationsBell';
export function Header() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState('');
  const isAuthenticated = useAuthStore(state => state.isAuthenticated);
  const user = useAuthStore(state => state.user);
  const {
    data: cart
  } = useCart();
  const logout = useLogout();
  function handleSearch(event) {
    event.preventDefault();
    navigate(searchTerm ? `/?q=${encodeURIComponent(searchTerm)}` : '/');
  }
  return <header className="bg-brand-500 text-white">
      <div className="mx-auto flex max-w-6xl items-center gap-6 px-4 py-3">
        <Link to="/" className="text-2xl font-bold tracking-tight">
          ShopCart
        </Link>

        <form onSubmit={handleSearch} className="flex-1">
          <div className="flex overflow-hidden rounded bg-white">
            <input type="search" value={searchTerm} onChange={event => setSearchTerm(event.target.value)} placeholder="Search products, brands, and categories" className="flex-1 px-4 py-2 text-sm text-gray-800 outline-none" />
            <button type="submit" className="bg-brand-600 px-4 text-sm font-medium hover:bg-brand-700">
              Search
            </button>
          </div>
        </form>

        <nav className="flex items-center gap-4 text-sm">
          <Link to="/cart" className="relative flex items-center gap-1">
            <span aria-hidden>🛒</span>
            Cart
            {cart && cart.totalItems > 0 && <span className="absolute -right-3 -top-2 rounded-full bg-white px-1.5 text-xs font-bold text-brand-600">
                {cart.totalItems}
              </span>}
          </Link>
          {isAuthenticated ? <>
              <NotificationsBell />
              <Link to="/orders" className="hover:underline">
                Orders
              </Link>
              {user?.role === 'Admin' && <>
                  <Link to="/admin/products" className="hover:underline">
                    Admin: Products
                  </Link>
                  <Link to="/admin/orders" className="hover:underline">
                    Admin: Orders
                  </Link>
                </>}
              <span>Hi, {user?.fullName ?? user?.email}</span>
              <button onClick={() => logout.mutate()} className="hover:underline">
                Logout
              </button>
            </> : <>
              <Link to="/login" className="hover:underline">
                Login
              </Link>
              <Link to="/register" className="hover:underline">
                Sign Up
              </Link>
            </>}
        </nav>
      </div>
    </header>;
}
