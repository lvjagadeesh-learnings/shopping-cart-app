import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useLogin } from '../hooks/useAuth';
export function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();
  const location = useLocation();
  const login = useLogin();
  const from = location.state?.from?.pathname ?? '/';
  async function handleSubmit(event) {
    event.preventDefault();
    try {
      await login.mutateAsync({
        email,
        password
      });
      navigate(from, {
        replace: true
      });
    } catch {
      // error state is surfaced via login.isError
    }
  }
  return <div className="mx-auto max-w-sm rounded border border-gray-200 bg-white p-6">
      <h1 className="mb-4 text-lg font-semibold text-gray-800">Login</h1>
      <form onSubmit={handleSubmit} className="flex flex-col gap-3">
        <label className="text-sm text-gray-700">
          Email
          <input type="email" required value={email} onChange={event => setEmail(event.target.value)} className="mt-1 w-full rounded border border-gray-300 px-3 py-2" />
        </label>
        <label className="text-sm text-gray-700">
          Password
          <input type="password" required value={password} onChange={event => setPassword(event.target.value)} className="mt-1 w-full rounded border border-gray-300 px-3 py-2" />
        </label>
        {login.isError && <p className="text-sm text-red-500">Invalid email or password.</p>}
        <button type="submit" disabled={login.isPending} className="bg-brand-500 hover:bg-brand-600 rounded py-2 font-medium text-white disabled:opacity-50">
          {login.isPending ? 'Logging in…' : 'Login'}
        </button>
      </form>
      <p className="mt-4 text-center text-sm text-gray-500">
        Don't have an account?{' '}
        <Link to="/register" className="text-brand-600 hover:underline">
          Sign up
        </Link>
      </p>
    </div>;
}
