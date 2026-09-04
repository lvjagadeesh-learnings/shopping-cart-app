import { Link } from 'react-router-dom';
export function ProductCard({
  product
}) {
  return <Link to={`/products/${product.slug}`} className="group flex flex-col overflow-hidden rounded border border-gray-200 bg-white transition hover:shadow-md">
      <div className="aspect-square overflow-hidden bg-gray-100">
        <img src={product.primaryImageUrl} alt={product.name} width={300} height={300} loading="lazy" className="h-full w-full object-cover transition group-hover:scale-105" />
      </div>
      <div className="flex flex-1 flex-col gap-1 p-3">
        <p className="line-clamp-2 text-sm text-gray-800">{product.name}</p>
        <div className="mt-auto flex items-baseline gap-2">
          <span className="text-brand-600 font-semibold">
            ${product.effectivePrice.toFixed(2)}
          </span>
          {product.discountPercent ? <span className="text-xs text-gray-400 line-through">${product.price.toFixed(2)}</span> : null}
        </div>
        <div className="flex items-center justify-between text-xs text-gray-500">
          <span>
            ⭐ {product.averageRating.toFixed(1)} ({product.ratingCount})
          </span>
          <span>{product.soldCount} sold</span>
        </div>
      </div>
    </Link>;
}
