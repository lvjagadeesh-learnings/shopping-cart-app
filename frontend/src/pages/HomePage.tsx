import { useSearchParams } from 'react-router-dom';
import { useState } from 'react';
import { useProducts } from '../hooks/useCatalog';
import { ProductCard } from '../components/ProductCard';
import { CategoryRail } from '../components/CategoryRail';
import type { ProductListQuery } from '../api/types';

const SORT_OPTIONS: { value: ProductListQuery['sort']; label: string }[] = [
  { value: '', label: 'Popular' },
  { value: 'newest', label: 'Newest' },
  { value: 'price_asc', label: 'Price: Low to High' },
  { value: 'price_desc', label: 'Price: High to Low' },
  { value: 'rating', label: 'Top Rated' },
];

export function HomePage() {
  const [searchParams] = useSearchParams();
  const [page, setPage] = useState(1);
  const query: ProductListQuery = {
    q: searchParams.get('q') ?? undefined,
    category: searchParams.get('category') ?? undefined,
    page,
    pageSize: 20,
  };
  const [sort, setSort] = useState<ProductListQuery['sort']>('');

  const { data, isLoading, isError } = useProducts({ ...query, sort });

  return (
    <div>
      <CategoryRail />

      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-lg font-semibold text-gray-800">
          {query.q ? `Results for "${query.q}"` : 'Recommended for you'}
        </h1>
        <select
          value={sort}
          onChange={(event) => {
            setSort(event.target.value as ProductListQuery['sort']);
            setPage(1);
          }}
          className="rounded border border-gray-300 px-2 py-1 text-sm"
        >
          {SORT_OPTIONS.map((option) => (
            <option key={option.label} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      {isLoading && <p className="text-gray-500">Loading products…</p>}
      {isError && <p className="text-red-500">Failed to load products.</p>}

      {data && (
        <>
          <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5">
            {data.items.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>

          {data.items.length === 0 && (
            <p className="mt-8 text-center text-gray-500">No products found.</p>
          )}

          {data.totalPages > 1 && (
            <div className="mt-6 flex justify-center gap-2">
              {Array.from({ length: data.totalPages }, (_, i) => i + 1).map((pageNumber) => (
                <button
                  key={pageNumber}
                  onClick={() => setPage(pageNumber)}
                  className={`h-8 w-8 rounded text-sm ${
                    pageNumber === page ? 'bg-brand-500 text-white' : 'bg-white text-gray-700'
                  }`}
                >
                  {pageNumber}
                </button>
              ))}
            </div>
          )}
        </>
      )}
    </div>
  );
}
