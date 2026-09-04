import { Link, useSearchParams } from 'react-router-dom';
import { useCategories } from '../hooks/useCatalog';
export function CategoryRail() {
  const {
    data: categories,
    isLoading
  } = useCategories();
  const [searchParams] = useSearchParams();
  const activeCategory = searchParams.get('category');
  if (isLoading || !categories) {
    return null;
  }
  return <nav className="mb-6 flex flex-wrap gap-3 rounded border border-gray-200 bg-white p-4">
      <Link to="/" className={`rounded-full px-3 py-1 text-sm ${!activeCategory ? 'bg-brand-500 text-white' : 'bg-gray-100 text-gray-700'}`}>
        All
      </Link>
      {categories.map(category => <Link key={category.id} to={`/?category=${category.slug}`} className={`rounded-full px-3 py-1 text-sm ${activeCategory === category.slug ? 'bg-brand-500 text-white' : 'bg-gray-100 text-gray-700'}`}>
          {category.name}
        </Link>)}
    </nav>;
}
