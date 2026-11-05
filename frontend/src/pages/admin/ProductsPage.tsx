import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Pencil, Plus, Search, Trash2 } from 'lucide-react'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { Link, useSearchParams } from 'react-router'
import { del, errorMessage, get } from '@/lib/api'
import { formatNumber, img, toFaDigits } from '@/lib/format'
import type { AdminCategory, AdminProductRow, PagedResult } from '@/lib/types'
import { EmptyState, Pagination, Spinner } from '@/components/ui'
import { PageHeader } from './AdminLayout'

export default function ProductsPage() {
  const [params, setParams] = useSearchParams()
  const [term, setTerm] = useState(params.get('q') ?? '')
  const qc = useQueryClient()
  const query = {
    q: params.get('q') || undefined, categoryId: params.get('categoryId') || undefined,
    isActive: params.get('isActive') || undefined, lowStock: params.get('lowStock') === 'true' || undefined,
    page: Number(params.get('page') ?? 1), pageSize: 20,
  }
  const { data, isLoading, isFetching } = useQuery({
    queryKey: ['admin', 'products', query], queryFn: () => get<PagedResult<AdminProductRow>>('/admin/products', query), placeholderData: keepPreviousData,
  })
  const { data: categories = [] } = useQuery({ queryKey: ['admin', 'categories'], queryFn: () => get<AdminCategory[]>('/admin/categories') })
  const remove = useMutation({
    mutationFn: (id: number) => del(`/admin/products/${id}`),
    onSuccess: () => { toast.success('محصول حذف شد'); qc.invalidateQueries({ queryKey: ['admin', 'products'] }) },
    onError: (e) => toast.error(errorMessage(e)),
  })
  const set = (k: string, v: string | null) => {
    const next = new URLSearchParams(params)
    if (v) next.set(k, v); else next.delete(k)
    if (k !== 'page') next.delete('page')
    setParams(next)
  }

  return (
    <div>
      <PageHeader title="محصولات" subtitle={data ? `${formatNumber(data.totalCount)} محصول` : undefined}
        actions={<Link to="/admin/products/new" className="btn-primary"><Plus size={18} /> محصول جدید</Link>} />
      <div className="card mb-4 flex flex-wrap gap-3 p-4">
        <form className="flex min-w-60 flex-1 items-center gap-2 rounded-lg border border-line px-3" onSubmit={(e) => { e.preventDefault(); set('q', term.trim() || null) }}>
          <Search size={18} className="text-muted" />
          <input className="h-10 flex-1 bg-transparent text-sm outline-none" placeholder="جستجوی عنوان محصول..." value={term} onChange={(e) => setTerm(e.target.value)} />
        </form>
        <select className="input w-auto" value={params.get('categoryId') ?? ''} onChange={(e) => set('categoryId', e.target.value || null)}>
          <option value="">همه دسته‌ها</option>
          {categories.map((c) => <option key={c.id} value={c.id}>{c.parentName ? `${c.parentName} › ` : ''}{c.name}</option>)}
        </select>
        <select className="input w-auto" value={params.get('isActive') ?? ''} onChange={(e) => set('isActive', e.target.value || null)}>
          <option value="">همه وضعیت‌ها</option><option value="true">فعال</option><option value="false">غیرفعال</option>
        </select>
        <label className="flex items-center gap-2 text-sm">
          <input type="checkbox" className="size-4 accent-brand-500" checked={params.get('lowStock') === 'true'} onChange={(e) => set('lowStock', e.target.checked ? 'true' : null)} />
          موجودی کم
        </label>
        {isFetching && <Spinner className="size-5" />}
      </div>

      <div className="card overflow-x-auto">
        <table className="w-full min-w-[900px] text-sm">
          <thead className="bg-surface text-xs text-muted">
            <tr><th className="p-3 text-right">محصول</th><th className="p-3 text-right">دسته</th><th className="p-3 text-right">قیمت (تومان)</th><th className="p-3 text-right">موجودی</th><th className="p-3 text-right">فروش</th><th className="p-3 text-right">وضعیت</th><th className="p-3" /></tr>
          </thead>
          <tbody className="divide-y divide-line">
            {data?.items.map((p) => (
              <tr key={p.id} className="hover:bg-surface/50">
                <td className="p-3">
                  <div className="flex items-center gap-3">
                    <img src={img(p.imageUrl)} alt="" className="size-12 rounded-lg border border-line object-contain" />
                    <div className="min-w-0">
                      <Link to={`/admin/products/${p.id}`} className="line-clamp-1 max-w-sm font-bold hover:text-brand-500">{p.title}</Link>
                      <div className="text-xs text-muted">{p.brandName ?? 'بدون برند'} {p.isAmazingOffer && <span className="mr-1 rounded bg-brand-50 px-1 text-brand-600">شگفت‌انگیز</span>}</div>
                    </div>
                  </div>
                </td>
                <td className="p-3 text-xs">{p.categoryName}</td>
                <td className="p-3">{formatNumber(p.finalPrice)}{p.discountPercent > 0 && <span className="mr-1 text-xs text-brand-500">({toFaDigits(p.discountPercent)}٪)</span>}</td>
                <td className="p-3"><span className={p.totalStock === 0 ? 'font-bold text-rose-600' : p.totalStock <= 5 ? 'font-bold text-amber-600' : ''}>{toFaDigits(p.totalStock)}</span></td>
                <td className="p-3">{toFaDigits(p.soldCount)}</td>
                <td className="p-3"><span className={`rounded-full px-2 py-1 text-xs ${p.isActive ? 'bg-emerald-50 text-emerald-700' : 'bg-gray-100 text-gray-500'}`}>{p.isActive ? 'فعال' : 'غیرفعال'}</span></td>
                <td className="p-3">
                  <div className="flex justify-end gap-1">
                    <Link to={`/admin/products/${p.id}`} className="rounded-lg p-2 text-muted hover:bg-surface hover:text-ink"><Pencil size={17} /></Link>
                    <button onClick={() => confirm(`«${p.title}» حذف شود؟`) && remove.mutate(p.id)} className="rounded-lg p-2 text-muted hover:bg-rose-50 hover:text-rose-600"><Trash2 size={17} /></button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        {isLoading && <div className="p-10 text-center"><Spinner /></div>}
        {data?.items.length === 0 && <EmptyState title="محصولی پیدا نشد" />}
      </div>
      {data && <Pagination page={data.page} totalPages={data.totalPages} onChange={(p) => set('page', String(p))} />}
    </div>
  )
}
