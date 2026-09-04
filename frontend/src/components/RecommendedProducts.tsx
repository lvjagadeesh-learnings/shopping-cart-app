import { Link } from 'react-router-dom';
import type { RecommendedProductResponse } from '../api/types';

export function RecommendedProducts({
  title,
  products,
  isLoading,
}: {
  title: string;
  products: RecommendedProductResponse[] | undefined;
  isLoading: boolean;
}) {
  if (isLoading || !products || products.length === 0) {
    return null;
  }

  return (
    <section className="mt-10">
      <h2 className="mb-4 text-lg font-semibold text-gray-900">{title}</h2>
      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
        {products.map((product) => (
          <Link
            key={product.productId}
            to={`/products/id/${product.productId}`}
            className="rounded border border-gray-200 bg-white p-2 hover:shadow-md"
          >
            <div className="aspect-square overflow-hidden rounded bg-gray-100">
              <img
                src={product.primaryImageUrl}
                alt={product.name}
                className="h-full w-full object-cover"
              />
            </div>
            <p className="mt-2 line-clamp-2 text-xs text-gray-700">{product.name}</p>
            <p className="text-brand-600 text-sm font-semibold">
              ${product.effectivePrice.toFixed(2)}
            </p>
            {!product.inStock && <p className="text-xs text-red-500">Out of stock</p>}
          </Link>
        ))}
      </div>
    </section>
  );
}
