import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { catalogApi } from '../api/catalogApi';

/** Recommendation/related-product widgets only know a productId — resolve to the slug-based route. */
export function ProductIdRedirectPage() {
  const {
    productId
  } = useParams();
  const navigate = useNavigate();
  const {
    data: product,
    isError
  } = useQuery({
    queryKey: ['product-by-id', productId],
    queryFn: () => catalogApi.getProductById(productId),
    enabled: Boolean(productId)
  });
  useEffect(() => {
    if (product) {
      navigate(`/products/${product.slug}`, {
        replace: true
      });
    }
  }, [product, navigate]);
  if (isError) {
    return <p className="text-red-500">Product not found.</p>;
  }
  return <p className="text-gray-500">Loading product…</p>;
}
