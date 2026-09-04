// Shared DTO types mirroring backend contracts (Auth, Catalog, Cart services)

export interface CategoryResponse {
  id: string;
  name: string;
  slug: string;
  iconUrl: string | null;
}

export interface ProductSummaryResponse {
  id: string;
  name: string;
  slug: string;
  price: number;
  effectivePrice: number;
  discountPercent: number | null;
  primaryImageUrl: string;
  averageRating: number;
  ratingCount: number;
  soldCount: number;
  inStock: boolean;
}

export interface ProductDetailResponse {
  id: string;
  name: string;
  slug: string;
  description: string;
  price: number;
  effectivePrice: number;
  discountPercent: number | null;
  primaryImageUrl: string;
  images: string[];
  averageRating: number;
  ratingCount: number;
  soldCount: number;
  stockQuantity: number;
  category: CategoryResponse;
  brandName: string | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface ProductListQuery {
  q?: string;
  category?: string;
  sort?: 'price_asc' | 'price_desc' | 'rating' | 'newest' | '';
  page?: number;
  pageSize?: number;
}

export interface CartItemResponse {
  id: string;
  productId: string;
  productName: string;
  productImageUrl: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface CartResponse {
  id: string;
  items: CartItemResponse[];
  subtotal: number;
  totalItems: number;
}

export interface AddCartItemRequest {
  productId: string;
  quantity: number;
}

export interface UpdateCartItemRequest {
  quantity: number;
}

export interface AuthUser {
  id: string;
  email: string;
  fullName?: string;
  role: 'Customer' | 'Admin';
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAtUtc: string;
  user: AuthUser;
}

export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// --- Order Service ---

export interface OrderItemResponse {
  id: string;
  productId: string;
  productName: string;
  productImageUrl: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export type OrderStatus =
  | 'Placed'
  | 'Paid'
  | 'Preparing'
  | 'Shipped'
  | 'OutForDelivery'
  | 'Delivered'
  | 'Cancelled';

export interface OrderResponse {
  id: string;
  status: OrderStatus;
  totalAmount: number;
  items: OrderItemResponse[];
  createdAtUtc: string;
}

export interface OrderSummaryResponse {
  id: string;
  status: OrderStatus;
  totalAmount: number;
  totalItems: number;
  createdAtUtc: string;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
}

// --- Review Service ---

export interface CreateReviewRequest {
  rating: number;
  comment?: string;
}

export interface ReviewResponse {
  id: string;
  productId: string;
  userId: string;
  rating: number;
  comment: string | null;
  createdAtUtc: string;
}

export interface ProductReviewSummaryResponse {
  productId: string;
  averageRating: number;
  reviewCount: number;
  reviews: ReviewResponse[];
}

// --- Notification Service ---

export interface NotificationResponse {
  id: string;
  eventType: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAtUtc: string;
}

// --- Recommendation Service ---

export interface RecordViewRequest {
  productId: string;
}

export interface RecommendedProductResponse {
  productId: string;
  name: string;
  primaryImageUrl: string;
  effectivePrice: number;
  averageRating: number;
  inStock: boolean;
}

// --- Admin: Catalog product CRUD ---

export interface UpsertProductRequest {
  name: string;
  description: string;
  price: number;
  discountPrice: number | null;
  primaryImageUrl: string;
  categoryId: string;
  brandId: string | null;
  stockQuantity: number;
}

// --- Admin: Inventory stock ---

export interface StockLevelResponse {
  productId: string;
  quantityOnHand: number;
  quantityReserved: number;
  quantityAvailable: number;
}

export interface AdjustStockRequest {
  quantityOnHand: number;
}

