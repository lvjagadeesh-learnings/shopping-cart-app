import { useAuthStore } from '../store/authStore';
export class ApiError extends Error {
  constructor(status, message) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}
async function request(baseUrl, path, options = {}) {
  const {
    method = 'GET',
    body,
    auth = false
  } = options;
  const headers = {};
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
    body: body !== undefined ? JSON.stringify(body) : undefined
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
    return undefined;
  }
  return await response.json();
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
  get: (path, opts) => request(authApiUrl, path, opts),
  post: (path, body, opts) => request(authApiUrl, path, {
    ...opts,
    method: 'POST',
    body
  })
};
export const catalogClient = {
  get: (path, opts) => request(catalogApiUrl, path, opts),
  post: (path, body, opts) => request(catalogApiUrl, path, {
    ...opts,
    method: 'POST',
    body,
    auth: true
  }),
  put: (path, body, opts) => request(catalogApiUrl, path, {
    ...opts,
    method: 'PUT',
    body,
    auth: true
  }),
  delete: (path, opts) => request(catalogApiUrl, path, {
    ...opts,
    method: 'DELETE',
    auth: true
  })
};
export const cartClient = {
  get: (path, opts) => request(cartApiUrl, path, {
    ...opts,
    auth: true
  }),
  post: (path, body, opts) => request(cartApiUrl, path, {
    ...opts,
    method: 'POST',
    body,
    auth: true
  }),
  put: (path, body, opts) => request(cartApiUrl, path, {
    ...opts,
    method: 'PUT',
    body,
    auth: true
  }),
  delete: (path, opts) => request(cartApiUrl, path, {
    ...opts,
    method: 'DELETE',
    auth: true
  })
};
export const orderClient = {
  get: (path, opts) => request(orderApiUrl, path, {
    ...opts,
    auth: true
  }),
  post: (path, body, opts) => request(orderApiUrl, path, {
    ...opts,
    method: 'POST',
    body,
    auth: true
  }),
  put: (path, body, opts) => request(orderApiUrl, path, {
    ...opts,
    method: 'PUT',
    body,
    auth: true
  })
};
export const reviewClient = {
  get: (path, opts) => request(reviewApiUrl, path, opts),
  post: (path, body, opts) => request(reviewApiUrl, path, {
    ...opts,
    method: 'POST',
    body,
    auth: true
  }),
  delete: (path, opts) => request(reviewApiUrl, path, {
    ...opts,
    method: 'DELETE',
    auth: true
  })
};
export const notificationClient = {
  get: (path, opts) => request(notificationApiUrl, path, {
    ...opts,
    auth: true
  }),
  put: (path, body, opts) => request(notificationApiUrl, path, {
    ...opts,
    method: 'PUT',
    body,
    auth: true
  })
};
export const recommendationClient = {
  get: (path, opts) => request(recommendationApiUrl, path, opts),
  post: (path, body, opts) => request(recommendationApiUrl, path, {
    ...opts,
    method: 'POST',
    body,
    auth: true
  })
};
export const inventoryClient = {
  get: (path, opts) => request(inventoryApiUrl, path, {
    ...opts,
    auth: true
  }),
  put: (path, body, opts) => request(inventoryApiUrl, path, {
    ...opts,
    method: 'PUT',
    body,
    auth: true
  })
};
