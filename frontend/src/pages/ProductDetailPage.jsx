import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useProduct } from '../hooks/useCatalog';
import { useAddToCart } from '../hooks/useCart';
import { useAuthStore } from '../store/authStore';
import { useRecordProductView, useRelatedProducts } from '../hooks/useRecommendations';
import { ReviewsSection } from '../components/ReviewsSection';
import { RecommendedProducts } from '../components/RecommendedProducts';
export function ProductDetailPage() {
  const {
    slug
  } = useParams();
  const navigate = useNavigate();
  const {
    data: product,
    isLoading,
    isError
  } = useProduct(slug);
  const [quantity, setQuantity] = useState(1);
  const [activeImage, setActiveImage] = useState(0);
  const isAuthenticated = useAuthStore(state => state.isAuthenticated);
  const addToCart = useAddToCart();
  const recordView = useRecordProductView();
  const {
    data: relatedProducts,
    isLoading: isLoadingRelated
  } = useRelatedProducts(product?.id);
  useEffect(() => {
    if (product && isAuthenticated) {
      recordView.mutate(product.id);
    }
    // Only fire once per product/auth change — recordView identity changes every render.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [product?.id, isAuthenticated]);
  if (isLoading) {
    return <p className="text-gray-500">Loading product…</p>;
  }
  if (isError || !product) {
    return <p className="text-red-500">Product not found.</p>;
  }
  const images = product.images.length > 0 ? product.images : [product.primaryImageUrl];
  function handleAddToCart() {
    if (!isAuthenticated) {
      navigate('/login');
      return;
    }
    addToCart.mutate({
      productId: product.id,
      quantity
    });
  }
  return <div className="grid grid-cols-1 gap-8 md:grid-cols-2">
      <div>
        <div className="aspect-square overflow-hidden rounded border border-gray-200 bg-white">
          <img src={images[activeImage]} alt={product.name} className="h-full w-full object-cover" />
        </div>
        {images.length > 1 && <div className="mt-2 flex gap-2">
            {images.map((image, index) => <button key={image} onClick={() => setActiveImage(index)} className={`h-16 w-16 overflow-hidden rounded border ${index === activeImage ? 'border-brand-500' : 'border-gray-200'}`}>
                <img src={image} alt="" className="h-full w-full object-cover" />
              </button>)}
          </div>}
      </div>

      <div>
        <h1 className="text-xl font-semibold text-gray-900">{product.name}</h1>
        <p className="mt-1 text-sm text-gray-500">
          ⭐ {product.averageRating.toFixed(1)} ({product.ratingCount} ratings) ·{' '}
          {product.soldCount} sold
        </p>

        <div className="mt-4 flex items-baseline gap-3 rounded bg-gray-50 p-4">
          <span className="text-brand-600 text-2xl font-bold">
            ${product.effectivePrice.toFixed(2)}
          </span>
          {product.discountPercent ? <>
              <span className="text-gray-400 line-through">${product.price.toFixed(2)}</span>
              <span className="rounded bg-brand-100 text-brand-700 px-2 py-0.5 text-xs font-semibold">
                -{product.discountPercent}%
              </span>
            </> : null}
        </div>

        <p className="mt-4 text-sm text-gray-600">{product.description}</p>

        <div className="mt-6 flex items-center gap-3">
          <label htmlFor="quantity" className="text-sm text-gray-700">
            Quantity
          </label>
          <input id="quantity" type="number" min={1} max={product.stockQuantity} value={quantity} onChange={event => setQuantity(Math.max(1, Number(event.target.value)))} className="w-20 rounded border border-gray-300 px-2 py-1" />
          <span className="text-xs text-gray-400">{product.stockQuantity} in stock</span>
        </div>

        <button onClick={handleAddToCart} disabled={addToCart.isPending || product.stockQuantity === 0} className="bg-brand-500 hover:bg-brand-600 mt-4 rounded px-6 py-2 font-medium text-white disabled:opacity-50">
          {product.stockQuantity === 0 ? 'Out of stock' : 'Add to Cart'}
        </button>

        {addToCart.isSuccess && <p className="mt-2 text-sm text-green-600">Added to cart!</p>}
        {addToCart.isError && <p className="mt-2 text-sm text-red-500">Failed to add to cart.</p>}
      </div>

      <div className="md:col-span-2">
        <RecommendedProducts title="You may also like" products={relatedProducts} isLoading={isLoadingRelated} />
        <ReviewsSection productId={product.id} />
      </div>
    </div>;
}
