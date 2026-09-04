import { beforeEach, describe, expect, it } from 'vitest';
import { useAuthStore } from './authStore';
const sampleAuth = {
  accessToken: 'access-token',
  refreshToken: 'refresh-token',
  expiresAtUtc: new Date().toISOString(),
  user: {
    id: '11111111-1111-1111-1111-111111111111',
    email: 'jane@example.com',
    fullName: 'Jane Doe',
    role: 'Customer'
  }
};
describe('authStore', () => {
  beforeEach(() => {
    useAuthStore.getState().clearSession();
  });
  it('starts unauthenticated', () => {
    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.user).toBeNull();
  });
  it('sets session on login', () => {
    useAuthStore.getState().setSession(sampleAuth);
    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(true);
    expect(state.accessToken).toBe('access-token');
    expect(state.user?.email).toBe('jane@example.com');
  });
  it('clears session on logout', () => {
    useAuthStore.getState().setSession(sampleAuth);
    useAuthStore.getState().clearSession();
    const state = useAuthStore.getState();
    expect(state.isAuthenticated).toBe(false);
    expect(state.accessToken).toBeNull();
    expect(state.user).toBeNull();
  });
});
