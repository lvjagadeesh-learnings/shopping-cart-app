import { authClient } from './httpClient';
export const authApi = {
  register: payload => authClient.post('/api/auth/register', payload),
  login: payload => authClient.post('/api/auth/login', payload),
  refresh: refreshToken => authClient.post('/api/auth/refresh', {
    refreshToken
  }),
  logout: refreshToken => authClient.post('/api/auth/logout', {
    refreshToken
  }),
  me: () => authClient.get('/api/auth/me', {
    auth: true
  })
};
