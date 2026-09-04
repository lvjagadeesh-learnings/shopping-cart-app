import { render } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        retry: false
      },
      mutations: {
        retry: false
      }
    }
  });
}
export function renderWithProviders(ui, {
  route = '/'
} = {}) {
  const queryClient = createTestQueryClient();
  function Wrapper({
    children
  }) {
    return <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={[route]}>{children}</MemoryRouter>
      </QueryClientProvider>;
  }
  return render(ui, {
    wrapper: Wrapper
  });
}
