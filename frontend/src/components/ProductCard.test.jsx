import { describe, expect, it } from 'vitest';
import { screen } from '@testing-library/react';
import { renderWithProviders } from '../test/test-utils';
import { ProductCard } from './ProductCard';
const product = {
  id: '11111111-1111-1111-1111-111111111111',
  name: 'Wireless Mouse',
  slug: 'wireless-mouse',
  price: 29.99,
  effectivePrice: 24.99,
  discountPercent: 17,
  primaryImageUrl: 'https://picsum.photos/seed/mouse/300',
  averageRating: 4.5,
  ratingCount: 120,
  soldCount: 500,
  inStock: true
};
describe('ProductCard', () => {
  it('renders product name, price, and rating', () => {
    renderWithProviders(<ProductCard product={product} />);
    expect(screen.getByText('Wireless Mouse')).toBeInTheDocument();
    expect(screen.getByText('$24.99')).toBeInTheDocument();
    expect(screen.getByText('$29.99')).toBeInTheDocument();
    expect(screen.getByText(/500 sold/)).toBeInTheDocument();
  });
  it('links to the product detail page', () => {
    renderWithProviders(<ProductCard product={product} />);
    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', '/products/wireless-mouse');
  });
  it('does not show the strikethrough price when there is no discount', () => {
    renderWithProviders(<ProductCard product={{
      ...product,
      discountPercent: null
    }} />);
    expect(screen.queryByText('$29.99')).not.toBeInTheDocument();
  });
});
