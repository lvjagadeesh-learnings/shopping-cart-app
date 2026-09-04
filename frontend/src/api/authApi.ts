import { authClient } from './httpClient';
import type { AuthResponse, LoginRequest, RegisterRequest, AuthUser } from './types';

export const authApi = {
  register: (payload: RegisterRequest) => authClient.post<AuthResponse>('/api/auth/register', payload),
  login: (payload: LoginRequest) => authClient.post<AuthResponse>('/api/auth/login', payload),
  refresh: (refreshToken: string) =>
    authClient.post<AuthResponse>('/api/auth/refresh', { refreshToken }),
  logout: (refreshToken: string) => authClient.post<void>('/api/auth/logout', { refreshToken }),
  me: () => authClient.get<AuthUser>('/api/auth/me', { auth: true }),
};
