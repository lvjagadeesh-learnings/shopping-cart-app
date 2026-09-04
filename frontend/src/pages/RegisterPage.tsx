import { type FormEvent, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useRegister } from '../hooks/useAuth';

export function RegisterPage() {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const navigate = useNavigate();
  const register = useRegister();

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    try {
      await register.mutateAsync({ fullName, email, password });
      navigate('/', { replace: true });
    } catch {
      // error state is surfaced via register.isError
    }
  }

  return (
    <div className="mx-auto max-w-sm rounded border border-gray-200 bg-white p-6">
      <h1 className="mb-4 text-lg font-semibold text-gray-800">Create an account</h1>
      <form onSubmit={handleSubmit} className="flex flex-col gap-3">
        <label className="text-sm text-gray-700">
          Full name
          <input
            type="text"
            required
            value={fullName}
            onChange={(event) => setFullName(event.target.value)}
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2"
          />
        </label>
        <label className="text-sm text-gray-700">
          Email
          <input
            type="email"
            required
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2"
          />
        </label>
        <label className="text-sm text-gray-700">
          Password
          <input
            type="password"
            required
            minLength={8}
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            className="mt-1 w-full rounded border border-gray-300 px-3 py-2"
          />
        </label>
        {register.isError && (
          <p className="text-sm text-red-500">Could not create account. Try a different email.</p>
        )}
        <button
          type="submit"
          disabled={register.isPending}
          className="bg-brand-500 hover:bg-brand-600 rounded py-2 font-medium text-white disabled:opacity-50"
        >
          {register.isPending ? 'Creating account…' : 'Sign up'}
        </button>
      </form>
      <p className="mt-4 text-center text-sm text-gray-500">
        Already have an account?{' '}
        <Link to="/login" className="text-brand-600 hover:underline">
          Login
        </Link>
      </p>
    </div>
  );
}
