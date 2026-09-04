import { describe, expect, it, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { renderWithProviders } from '../test/test-utils';
import { CartPage } from './CartPage';
import { cartApi } from '../api/cartApi';
import { useAuthStore } from '../store/authStore';
import type { AuthResponse, CartResponse } from '../api/types';

vi.mock('../api/cartApi', () => ({
  cartApi: {
    getCart: vi.fn(),
    updateItem: vi.fn(),
    removeItem: vi.fn(),
  },
}));

const sampleAuth: AuthResponse = {
  accessToken: 'token',
  refreshToken: 'refresh',
  expiresAtUtc: new Date().toISOString(),
  user: { id: '1', email: 'jane@example.com', fullName: 'Jane', role: 'Customer' },
};

describe('CartPage', () => {
  beforeEach(() => {
    useAuthStore.getState().setSession(sampleAuth);
    vi.mocked(cartApi.getCart).mockReset();
  });

  it('shows an empty cart message when there are no items', async () => {
    vi.mocked(cartApi.getCart).mockResolvedValue({
      id: 'cart-1',
      items: [],
      subtotal: 0,
      totalItems: 0,
    });

    renderWithProviders(<CartPage />);

    expect(await screen.findByText(/your cart is empty/i)).toBeInTheDocument();
  });

  it('renders cart items and subtotal', async () => {
    const cart: CartResponse = {
      id: 'cart-1',
      items: [
        {
          id: 'item-1',
          productId: 'product-1',
          productName: 'Wireless Mouse',
          productImageUrl: 'https://picsum.photos/seed/mouse/64',
          unitPrice: 24.99,
          quantity: 2,
          lineTotal: 49.98,
        },
      ],
      subtotal: 49.98,
      totalItems: 2,
    };
    vi.mocked(cartApi.getCart).mockResolvedValue(cart);

    renderWithProviders(<CartPage />);

    await waitFor(() => {
      expect(screen.getByText('Wireless Mouse')).toBeInTheDocument();
    });
    expect(screen.getAllByText('$49.98')).toHaveLength(3);
  });
});
