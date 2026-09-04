import { useState } from 'react';
import { useCategories, useProducts } from '../hooks/useCatalog';
import { useAdminAdjustStock, useAdminCreateProduct, useAdminDeleteProduct, useAdminStockLevel, useAdminUpdateProduct } from '../hooks/useAdmin';
const emptyForm = {
  name: '',
  description: '',
  price: 0,
  discountPrice: null,
  primaryImageUrl: 'https://picsum.photos/seed/new-product/400',
  categoryId: '',
  brandId: null,
  stockQuantity: 0
};
export function AdminProductsPage() {
  const {
    data: categories
  } = useCategories();
  const {
    data: products,
    isLoading
  } = useProducts({
    page: 1,
    pageSize: 50
  });
  const createProduct = useAdminCreateProduct();
  const updateProduct = useAdminUpdateProduct();
  const deleteProduct = useAdminDeleteProduct();
  const [editingId, setEditingId] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [stockProductId, setStockProductId] = useState(null);
  const {
    data: stock
  } = useAdminStockLevel(stockProductId ?? undefined);
  const adjustStock = useAdminAdjustStock();
  const [stockValue, setStockValue] = useState(0);
  function resetForm() {
    setEditingId(null);
    setForm(emptyForm);
  }
  function handleSubmit(event) {
    event.preventDefault();
    if (editingId) {
      updateProduct.mutate({
        productId: editingId,
        payload: form
      }, {
        onSuccess: resetForm
      });
    } else {
      createProduct.mutate(form, {
        onSuccess: resetForm
      });
    }
  }
  return <div>
      <h1 className="mb-6 text-lg font-semibold text-gray-800">Manage Products</h1>

      <form onSubmit={handleSubmit} className="mb-8 grid grid-cols-1 gap-3 rounded border border-gray-200 bg-white p-4 sm:grid-cols-2">
        <h2 className="col-span-full text-sm font-semibold text-gray-700">
          {editingId ? 'Edit Product' : 'New Product'}
        </h2>
        <input required placeholder="Name" value={form.name} onChange={e => setForm({
        ...form,
        name: e.target.value
      })} className="rounded border border-gray-300 px-3 py-2 text-sm" />
        <select required value={form.categoryId} onChange={e => setForm({
        ...form,
        categoryId: e.target.value
      })} className="rounded border border-gray-300 px-3 py-2 text-sm">
          <option value="">Select category…</option>
          {categories?.map(category => <option key={category.id} value={category.id}>
              {category.name}
            </option>)}
        </select>
        <textarea required placeholder="Description" value={form.description} onChange={e => setForm({
        ...form,
        description: e.target.value
      })} className="col-span-full rounded border border-gray-300 px-3 py-2 text-sm" />
        <input type="number" min={0} step="0.01" placeholder="Price" value={form.price} onChange={e => setForm({
        ...form,
        price: Number(e.target.value)
      })} className="rounded border border-gray-300 px-3 py-2 text-sm" />
        <input type="number" min={0} step="0.01" placeholder="Discount price (optional)" value={form.discountPrice ?? ''} onChange={e => setForm({
        ...form,
        discountPrice: e.target.value ? Number(e.target.value) : null
      })} className="rounded border border-gray-300 px-3 py-2 text-sm" />
        <input placeholder="Image URL" value={form.primaryImageUrl} onChange={e => setForm({
        ...form,
        primaryImageUrl: e.target.value
      })} className="rounded border border-gray-300 px-3 py-2 text-sm" />
        <input type="number" min={0} placeholder="Initial stock quantity" value={form.stockQuantity} onChange={e => setForm({
        ...form,
        stockQuantity: Number(e.target.value)
      })} className="rounded border border-gray-300 px-3 py-2 text-sm" />
        <div className="col-span-full flex gap-3">
          <button type="submit" disabled={createProduct.isPending || updateProduct.isPending} className="bg-brand-500 hover:bg-brand-600 rounded px-4 py-2 text-sm font-medium text-white disabled:opacity-50">
            {editingId ? 'Save Changes' : 'Create Product'}
          </button>
          {editingId && <button type="button" onClick={resetForm} className="rounded border border-gray-300 px-4 py-2 text-sm text-gray-700">
              Cancel
            </button>}
        </div>
      </form>

      {isLoading ? <p className="text-gray-500">Loading products…</p> : <ul className="divide-y divide-gray-200 rounded border border-gray-200 bg-white">
          {products?.items.map(product => <li key={product.id} className="flex items-center gap-4 p-4">
              <img src={product.primaryImageUrl} alt={product.name} className="h-12 w-12 rounded object-cover" />
              <div className="flex-1">
                <p className="text-sm font-medium text-gray-800">{product.name}</p>
                <p className="text-xs text-gray-500">${product.effectivePrice.toFixed(2)}</p>
              </div>
              <button onClick={() => {
          setStockProductId(product.id);
          setStockValue(0);
        }} className="text-sm text-gray-600 hover:underline">
                Stock
              </button>
              <button onClick={() => {
          setEditingId(product.id);
          setForm({
            name: product.name,
            description: '',
            price: product.price,
            discountPrice: null,
            primaryImageUrl: product.primaryImageUrl,
            categoryId: '',
            brandId: null,
            stockQuantity: 0
          });
        }} className="text-brand-600 text-sm hover:underline">
                Edit
              </button>
              <button onClick={() => deleteProduct.mutate(product.id)} className="text-sm text-red-500 hover:underline">
                Delete
              </button>
            </li>)}
        </ul>}

      {stockProductId && <div className="fixed inset-0 flex items-center justify-center bg-black/30">
          <div className="w-80 rounded bg-white p-4">
            <h3 className="mb-2 text-sm font-semibold text-gray-800">Adjust Stock</h3>
            {stock && <p className="mb-2 text-xs text-gray-500">
                On hand: {stock.quantityOnHand} · Reserved: {stock.quantityReserved} · Available:{' '}
                {stock.quantityAvailable}
              </p>}
            <input type="number" min={0} value={stockValue} onChange={e => setStockValue(Number(e.target.value))} className="mb-3 w-full rounded border border-gray-300 px-3 py-2 text-sm" placeholder="New quantity on hand" />
            <div className="flex justify-end gap-2">
              <button onClick={() => setStockProductId(null)} className="rounded border border-gray-300 px-3 py-1.5 text-sm text-gray-700">
                Close
              </button>
              <button onClick={() => adjustStock.mutate({
            productId: stockProductId,
            payload: {
              quantityOnHand: stockValue
            }
          }, {
            onSuccess: () => setStockProductId(null)
          })} className="bg-brand-500 hover:bg-brand-600 rounded px-3 py-1.5 text-sm text-white">
                Save
              </button>
            </div>
          </div>
        </div>}
    </div>;
}
