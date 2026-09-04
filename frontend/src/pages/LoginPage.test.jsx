import { describe, expect, it, vi, beforeEach } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { renderWithProviders } from '../test/test-utils';
import { LoginPage } from './LoginPage';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';
vi.mock('../api/authApi', () => ({
  authApi: {
    login: vi.fn()
  }
}));
describe('LoginPage', () => {
  beforeEach(() => {
    useAuthStore.getState().clearSession();
    vi.mocked(authApi.login).mockReset();
  });
  it('renders email and password fields', () => {
    renderWithProviders(<LoginPage />);
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
  });
  it('logs in and stores the session on submit', async () => {
    vi.mocked(authApi.login).mockResolvedValue({
      accessToken: 'token',
      refreshToken: 'refresh',
      expiresAtUtc: new Date().toISOString(),
      user: {
        id: '1',
        email: 'jane@example.com',
        fullName: 'Jane',
        role: 'Customer'
      }
    });
    const user = userEvent.setup();
    renderWithProviders(<LoginPage />);
    await user.type(screen.getByLabelText(/email/i), 'jane@example.com');
    await user.type(screen.getByLabelText(/password/i), 'password123');
    await user.click(screen.getByRole('button', {
      name: /login/i
    }));
    await waitFor(() => {
      expect(useAuthStore.getState().isAuthenticated).toBe(true);
    });
    expect(authApi.login).toHaveBeenCalledWith({
      email: 'jane@example.com',
      password: 'password123'
    });
  });
  it('shows an error message when login fails', async () => {
    vi.mocked(authApi.login).mockRejectedValue(new Error('Invalid credentials'));
    const user = userEvent.setup();
    renderWithProviders(<LoginPage />);
    await user.type(screen.getByLabelText(/email/i), 'jane@example.com');
    await user.type(screen.getByLabelText(/password/i), 'wrong-password');
    await user.click(screen.getByRole('button', {
      name: /login/i
    }));
    expect(await screen.findByText(/invalid email or password/i)).toBeInTheDocument();
  });
});
