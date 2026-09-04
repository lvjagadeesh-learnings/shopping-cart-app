import { Routes, Route } from 'react-router-dom';
import { Layout } from './components/layout/Layout';
import { ProtectedRoute } from './components/ProtectedRoute';
import { AdminRoute } from './components/AdminRoute';
import { HomePage } from './pages/HomePage';
import { ProductDetailPage } from './pages/ProductDetailPage';
import { ProductIdRedirectPage } from './pages/ProductIdRedirectPage';
import { CartPage } from './pages/CartPage';
import { CheckoutPage } from './pages/CheckoutPage';
import { OrderConfirmationPage } from './pages/OrderConfirmationPage';
import { OrderHistoryPage } from './pages/OrderHistoryPage';
import { OrderDetailPage } from './pages/OrderDetailPage';
import { AdminProductsPage } from './pages/AdminProductsPage';
import { AdminOrdersPage } from './pages/AdminOrdersPage';
import { LoginPage } from './pages/LoginPage';
import { RegisterPage } from './pages/RegisterPage';
function App() {
  return <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/products/:slug" element={<ProductDetailPage />} />
        <Route path="/products/id/:productId" element={<ProductIdRedirectPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route element={<ProtectedRoute />}>
          <Route path="/cart" element={<CartPage />} />
          <Route path="/checkout" element={<CheckoutPage />} />
          <Route path="/orders" element={<OrderHistoryPage />} />
          <Route path="/orders/:orderId" element={<OrderDetailPage />} />
          <Route path="/orders/:orderId/confirmation" element={<OrderConfirmationPage />} />
        </Route>
        <Route element={<AdminRoute />}>
          <Route path="/admin/products" element={<AdminProductsPage />} />
          <Route path="/admin/orders" element={<AdminOrdersPage />} />
        </Route>
      </Route>
    </Routes>;
}
export default App;
