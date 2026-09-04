import { useMutation } from '@tanstack/react-query';
import { authApi } from '../api/authApi';
import { useAuthStore } from '../store/authStore';
export function useLogin() {
  const setSession = useAuthStore(state => state.setSession);
  return useMutation({
    mutationFn: payload => authApi.login(payload),
    onSuccess: data => setSession(data)
  });
}
export function useRegister() {
  const setSession = useAuthStore(state => state.setSession);
  return useMutation({
    mutationFn: payload => authApi.register(payload),
    onSuccess: data => setSession(data)
  });
}
export function useLogout() {
  const refreshToken = useAuthStore(state => state.refreshToken);
  const clearSession = useAuthStore(state => state.clearSession);
  return useMutation({
    mutationFn: () => refreshToken ? authApi.logout(refreshToken) : Promise.resolve(),
    onSettled: () => clearSession()
  });
}
