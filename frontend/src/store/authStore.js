import { create } from 'zustand';
import { persist } from 'zustand/middleware';
export const useAuthStore = create()(persist(set => ({
  accessToken: null,
  refreshToken: null,
  user: null,
  isAuthenticated: false,
  setSession: auth => set({
    accessToken: auth.accessToken,
    refreshToken: auth.refreshToken,
    user: auth.user,
    isAuthenticated: true
  }),
  clearSession: () => set({
    accessToken: null,
    refreshToken: null,
    user: null,
    isAuthenticated: false
  })
}), {
  name: 'shopping-cart-auth'
}));
