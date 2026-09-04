import { useState } from 'react';
import { useAuthStore } from '../store/authStore';
import { useCreateReview, useDeleteReview, useProductReviews } from '../hooks/useReviews';
export function ReviewsSection({
  productId
}) {
  const {
    data: summary,
    isLoading
  } = useProductReviews(productId);
  const user = useAuthStore(state => state.user);
  const isAuthenticated = useAuthStore(state => state.isAuthenticated);
  const createReview = useCreateReview(productId);
  const deleteReview = useDeleteReview(productId);
  const [rating, setRating] = useState(5);
  const [comment, setComment] = useState('');
  function handleSubmit(event) {
    event.preventDefault();
    createReview.mutate({
      rating,
      comment: comment.trim() || undefined
    }, {
      onSuccess: () => setComment('')
    });
  }
  if (isLoading) {
    return <p className="text-gray-500">Loading reviews…</p>;
  }
  return <section className="mt-10">
      <h2 className="mb-4 text-lg font-semibold text-gray-900">
        Reviews {summary && summary.reviewCount > 0 && <span className="text-sm font-normal text-gray-500">
            ⭐ {summary.averageRating.toFixed(1)} ({summary.reviewCount})
          </span>}
      </h2>

      {isAuthenticated && <form onSubmit={handleSubmit} className="mb-6 rounded border border-gray-200 bg-white p-4">
          <label className="mb-2 block text-sm text-gray-700">
            Rating
            <select value={rating} onChange={event => setRating(Number(event.target.value))} className="ml-2 rounded border border-gray-300 px-2 py-1">
              {[5, 4, 3, 2, 1].map(value => <option key={value} value={value}>
                  {value} ⭐
                </option>)}
            </select>
          </label>
          <textarea value={comment} onChange={event => setComment(event.target.value)} placeholder="Share your thoughts about this product…" className="mb-2 w-full rounded border border-gray-300 px-3 py-2 text-sm" rows={3} />
          <button type="submit" disabled={createReview.isPending} className="bg-brand-500 hover:bg-brand-600 rounded px-4 py-2 text-sm font-medium text-white disabled:opacity-50">
            Submit Review
          </button>
          {createReview.isError && <p className="mt-2 text-sm text-red-500">
              Failed to submit review. You may have already reviewed this product.
            </p>}
        </form>}

      {!summary || summary.reviews.length === 0 ? <p className="text-sm text-gray-500">No reviews yet. Be the first to review!</p> : <ul className="space-y-4">
          {summary.reviews.map(review => <li key={review.id} className="rounded border border-gray-200 bg-white p-4">
              <div className="flex items-center justify-between">
                <span className="text-sm font-medium text-gray-800">{'⭐'.repeat(review.rating)}</span>
                <span className="text-xs text-gray-400">
                  {new Date(review.createdAtUtc).toLocaleDateString()}
                </span>
              </div>
              {review.comment && <p className="mt-2 text-sm text-gray-600">{review.comment}</p>}
              {user?.id === review.userId && <button onClick={() => deleteReview.mutate(review.id)} className="mt-2 text-xs text-red-500 hover:underline">
                  Delete
                </button>}
            </li>)}
        </ul>}
    </section>;
}
