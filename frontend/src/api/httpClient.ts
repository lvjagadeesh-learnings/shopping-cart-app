import { useAuthStore } from '../store/authStore';

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

interface RequestOptions {
  method?: 'GET' | 'POST' | 'PUT' | 'DELETE';
  body?: unknown;
  auth?: boolean;
}

async function request<T>(baseUrl: string, path: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, auth = false } = options;

  const headers: Record<string, string> = {};
  if (body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }
  if (auth) {
    const token = useAuthStore.getState().accessToken;
    if (token) {
      headers['Authorization'] = `Bearer ${token}`;
    }
  }

  const response = await fetch(`${baseUrl}${path}`, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!response.ok) {
    let message = response.statusText;
    try {
      const data = await response.json();
      message = data?.title ?? data?.message ?? message;
    } catch {
      // response had no JSON body
    }
    throw new ApiError(response.status, message);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export const authApiUrl = import.meta.env.VITE_AUTH_API_URL ?? 'http://localhost:5213';
export const catalogApiUrl = import.meta.env.VITE_CATALOG_API_URL ?? 'http://localhost:5005';
export const cartApiUrl = import.meta.env.VITE_CART_API_URL ?? 'http://localhost:5140';
export const orderApiUrl = import.meta.env.VITE_ORDER_API_URL ?? 'http://localhost:5182';
export const reviewApiUrl = import.meta.env.VITE_REVIEW_API_URL ?? 'http://localhost:5162';
export const notificationApiUrl = import.meta.env.VITE_NOTIFICATION_API_URL ?? 'http://localhost:5277';
export const recommendationApiUrl = import.meta.env.VITE_RECOMMENDATION_API_URL ?? 'http://localhost:5106';
export const inventoryApiUrl = import.meta.env.VITE_INVENTORY_API_URL ?? 'http://localhost:5238';

export const authClient = {
  get: <T>(path: string, opts?: RequestOptions) => request<T>(authApiUrl, path, opts),
  post: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(authApiUrl, path, { ...opts, method: 'POST', body }),
};

export const catalogClient = {
  get: <T>(path: string, opts?: RequestOptions) => request<T>(catalogApiUrl, path, opts),
  post: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(catalogApiUrl, path, { ...opts, method: 'POST', body, auth: true }),
  put: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(catalogApiUrl, path, { ...opts, method: 'PUT', body, auth: true }),
  delete: <T>(path: string, opts?: RequestOptions) =>
    request<T>(catalogApiUrl, path, { ...opts, method: 'DELETE', auth: true }),
};

export const cartClient = {
  get: <T>(path: string, opts?: RequestOptions) =>
    request<T>(cartApiUrl, path, { ...opts, auth: true }),
  post: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(cartApiUrl, path, { ...opts, method: 'POST', body, auth: true }),
  put: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(cartApiUrl, path, { ...opts, method: 'PUT', body, auth: true }),
  delete: <T>(path: string, opts?: RequestOptions) =>
    request<T>(cartApiUrl, path, { ...opts, method: 'DELETE', auth: true }),
};

export const orderClient = {
  get: <T>(path: string, opts?: RequestOptions) =>
    request<T>(orderApiUrl, path, { ...opts, auth: true }),
  post: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(orderApiUrl, path, { ...opts, method: 'POST', body, auth: true }),
  put: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(orderApiUrl, path, { ...opts, method: 'PUT', body, auth: true }),
};

export const reviewClient = {
  get: <T>(path: string, opts?: RequestOptions) => request<T>(reviewApiUrl, path, opts),
  post: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(reviewApiUrl, path, { ...opts, method: 'POST', body, auth: true }),
  delete: <T>(path: string, opts?: RequestOptions) =>
    request<T>(reviewApiUrl, path, { ...opts, method: 'DELETE', auth: true }),
};

export const notificationClient = {
  get: <T>(path: string, opts?: RequestOptions) =>
    request<T>(notificationApiUrl, path, { ...opts, auth: true }),
  put: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(notificationApiUrl, path, { ...opts, method: 'PUT', body, auth: true }),
};

export const recommendationClient = {
  get: <T>(path: string, opts?: RequestOptions) => request<T>(recommendationApiUrl, path, opts),
  post: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(recommendationApiUrl, path, { ...opts, method: 'POST', body, auth: true }),
};

export const inventoryClient = {
  get: <T>(path: string, opts?: RequestOptions) =>
    request<T>(inventoryApiUrl, path, { ...opts, auth: true }),
  put: <T>(path: string, body?: unknown, opts?: RequestOptions) =>
    request<T>(inventoryApiUrl, path, { ...opts, method: 'PUT', body, auth: true }),
};
