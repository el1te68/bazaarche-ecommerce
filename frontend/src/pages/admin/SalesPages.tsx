import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { clsx } from 'clsx'
import { ArrowRight, Check, Pencil, Plus, Search, Trash2, X } from 'lucide-react'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { Link, useParams, useSearchParams } from 'react-router'
import { del, errorMessage, get, post, put } from '@/lib/api'
import { formatDate, formatDateTime, formatNumber, img, orderStatusLabel, toFaDigits } from '@/lib/format'
import type { AdminOrderRow, AdminReview, AdminUser, Coupon, CouponType, OrderDetail, OrderStatus, PagedResult, UserRole } from '@/lib/types'
import { useAuthStore } from '@/stores/auth'
import { EmptyState, Field, Modal, PageLoader, Pagination, Price, RatingStars, Spinner, StatusBadge, Toggle } from '@/components/ui'
import { PageHeader } from './AdminLayout'

function useListParams() {
  const [params, setParams] = useSearchParams()
  const set = (k: string, v: string | null) => {
    const next = new URLSearchParams(params)
    if (v) next.set(k, v); else next.delete(k)
    if (k !== 'page') next.delete('page')
    setParams(next)
  }
  return { params, set, page: Number(params.get('page') ?? 1) }
}

function SearchInput({ initial, onSearch, placeholder }: { initial: string; onSearch: (v: string | null) => void; placeholder: string }) {
  const [term, setTerm] = useState(initial)
  return (
    <form className="flex min-w-60 flex-1 items-center gap-2 rounded-lg border border-line bg-white px-3" onSubmit={(e) => { e.preventDefault(); onSearch(term.trim() || null) }}>
      <Search size={18} className="text-muted" />
      <input className="h-10 flex-1 bg-transparent text-sm outline-none" placeholder={placeholder} value={term} onChange={(e) => setTerm(e.target.value)} />
    </form>
  )
}

const statuses: OrderStatus[] = ['PendingPayment', 'Processing', 'Shipped', 'Delivered', 'Cancelled']

// ---------------- Orders ----------------

export function OrdersPage() {
  const { params, set, page } = useListParams()
  const query = { q: params.get('q') || undefined, status: params.get('status') || undefined, page, pageSize: 20 }
  const { data, isLoading, isFetching } = useQuery({
    queryKey: ['admin', 'orders', query], queryFn: () => get<PagedResult<AdminOrderRow>>('/admin/orders', query), placeholderData: keepPreviousData,
  })
  return (
    <div>
      <PageHeader title="سفارش‌ها" subtitle={data ? `${formatNumber(data.totalCount)} سفارش` : undefined} />
      <div className="card mb-4 space-y-3 p-4">
        <div className="scrollbar-none flex gap-2 overflow-x-auto">
          <button onClick={() => set('status', null)} className={clsx('chip shrink-0 py-1.5', !query.status && 'border-brand-500 bg-brand-50 text-brand-600')}>همه</button>
          {statuses.map((s) => (
            <button key={s} onClick={() => set('status', s)} className={clsx('chip shrink-0 py-1.5', query.status === s && 'border-brand-500 bg-brand-50 text-brand-600')}>{orderStatusLabel[s]}</button>
          ))}
        </div>
        <div className="flex items-center gap-3">
          <SearchInput initial={params.get('q') ?? ''} onSearch={(v) => set('q', v)} placeholder="شماره سفارش، نام یا موبایل مشتری..." />
          {isFetching && <Spinner className="size-5" />}
        </div>
      </div>
      <div className="card overflow-x-auto">
        <table className="w-full min-w-[860px] text-sm">
          <thead className="bg-surface text-xs text-muted"><tr><th className="p-3 text-right">شماره</th><th className="p-3 text-right">مشتری</th><th className="p-3 text-right">تعداد</th><th className="p-3 text-right">مبلغ (تومان)</th><th className="p-3 text-right">ارسال</th><th className="p-3 text-right">وضعیت</th><th className="p-3 text-right">تاریخ</th></tr></thead>
          <tbody className="divide-y divide-line">
            {data?.items.map((o) => (
              <tr key={o.id} className="hover:bg-surface/50">
                <td className="p-3"><Link to={`/admin/orders/${o.id}`} className="font-bold text-sky-600" dir="ltr">{o.orderNumber}</Link></td>
                <td className="p-3">{o.customerName}<div className="text-xs text-muted" dir="ltr">{toFaDigits(o.customerPhone)}</div></td>
                <td className="p-3">{toFaDigits(o.itemsCount)}</td>
                <td className="p-3 font-bold">{formatNumber(o.payableAmount)}</td>
                <td className="p-3 text-xs">{o.shippingMethod === 'Express' ? 'اکسپرس' : 'عادی'}</td>
                <td className="p-3"><StatusBadge status={o.status} /></td>
                <td className="p-3 text-xs text-muted">{formatDateTime(o.createdAt)}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isLoading && <div className="p-10 text-center"><Spinner /></div>}
        {data?.items.length === 0 && <EmptyState title="سفارشی پیدا نشد" />}
      </div>
      {data && <Pagination page={data.page} totalPages={data.totalPages} onChange={(p) => set('page', String(p))} />}
    </div>
  )
}

const transitions: Record<OrderStatus, OrderStatus[]> = {
  PendingPayment: ['Cancelled'], Processing: ['Shipped', 'Cancelled'], Shipped: ['Delivered'], Delivered: [], Cancelled: [],
}

export function OrderDetailPage() {
  const { id } = useParams()
  const qc = useQueryClient()
  const { data: o, isLoading } = useQuery({ queryKey: ['admin', 'order', id], queryFn: () => get<OrderDetail>(`/admin/orders/${id}`) })
  const [tracking, setTracking] = useState<string>()
  const update = useMutation({
    mutationFn: (body: { status: OrderStatus; trackingCode?: string }) => put<OrderDetail>(`/admin/orders/${id}/status`, body),
    onSuccess: (d) => { qc.setQueryData(['admin', 'order', id], d); qc.invalidateQueries({ queryKey: ['admin', 'orders'] }); toast.success('سفارش به‌روز شد') },
    onError: (e) => toast.error(errorMessage(e)),
  })
  if (isLoading || !o) return <PageLoader />
  const trackingValue = tracking ?? o.trackingCode ?? ''

  return (
    <div className="space-y-4">
      <PageHeader title={`سفارش ${o.orderNumber}`} subtitle={formatDateTime(o.createdAt)}
        actions={<Link to="/admin/orders" className="btn-ghost"><ArrowRight size={18} /> بازگشت</Link>} />
      <div className="grid gap-4 xl:grid-cols-[1fr_340px]">
        <div className="space-y-4">
          <div className="card p-5">
            <h2 className="mb-4 font-bold">اقلام سفارش</h2>
            <ul className="divide-y divide-line">
              {o.items.map((i) => (
                <li key={i.variantId} className="flex items-center gap-4 py-3 text-sm">
                  <img src={img(i.imageUrl)} alt="" className="size-14 rounded-lg border border-line object-contain" />
                  <div className="flex-1"><div className="line-clamp-1 font-bold">{i.title}</div>
                    <div className="text-xs text-muted">{i.colorName && `رنگ: ${i.colorName} • `}تعداد: {toFaDigits(i.quantity)} • قیمت واحد: {formatNumber(i.unitFinalPrice)}</div></div>
                  <Price value={i.unitFinalPrice * i.quantity} size="sm" />
                </li>
              ))}
            </ul>
            <div className="mt-4 space-y-2 border-t border-line pt-4 text-sm">
              <div className="flex justify-between text-muted"><span>جمع کالاها</span><span>{formatNumber(o.itemsTotal)}</span></div>
              <div className="flex justify-between text-muted"><span>تخفیف کالاها</span><span>{formatNumber(o.productsDiscount)}</span></div>
              {o.couponCode && <div className="flex justify-between text-muted"><span>کد تخفیف {o.couponCode}</span><span>{formatNumber(o.couponDiscount)}</span></div>}
              <div className="flex justify-between text-muted"><span>ارسال ({o.shippingMethod === 'Express' ? 'اکسپرس' : 'عادی'})</span><span>{formatNumber(o.shippingCost)}</span></div>
              <div className="flex justify-between font-bold"><span>مبلغ نهایی</span><Price value={o.payableAmount} /></div>
            </div>
          </div>
          <div className="card p-5 text-sm">
            <h2 className="mb-3 font-bold">تراکنش‌ها</h2>
            {o.payments.map((p) => (
              <div key={p.id} className="flex flex-wrap justify-between gap-2 border-b border-line py-2 last:border-0">
                <span>{formatDateTime(p.createdAt)}</span>
                <span className={p.status === 'Succeeded' ? 'text-success' : p.status === 'Failed' ? 'text-brand-500' : 'text-amber-600'}>{p.status === 'Succeeded' ? 'موفق' : p.status === 'Failed' ? 'ناموفق' : 'در انتظار'}</span>
                <span className="text-muted" dir="ltr">{p.cardMask}</span>
                <span className="text-muted">{p.referenceId && `پیگیری: ${toFaDigits(p.referenceId)}`}</span>
              </div>
            ))}
          </div>
        </div>
        <aside className="space-y-4">
          <div className="card space-y-4 p-5">
            <div className="flex items-center justify-between"><h2 className="font-bold">وضعیت</h2><StatusBadge status={o.status} /></div>
            {transitions[o.status].length > 0 ? (
              <div className="flex flex-wrap gap-2">
                {transitions[o.status].map((s) => (
                  <button key={s} disabled={update.isPending} className={s === 'Cancelled' ? 'btn-ghost text-rose-600' : 'btn-primary'}
                    onClick={() => (s !== 'Cancelled' || confirm('سفارش لغو و موجودی به انبار برگردد؟')) && update.mutate({ status: s, trackingCode: trackingValue })}>
                    تغییر به «{orderStatusLabel[s]}»
                  </button>
                ))}
              </div>
            ) : <p className="text-xs text-muted">این سفارش در وضعیت نهایی است.</p>}
            <Field label="کد رهگیری مرسوله">
              <div className="flex gap-2">
                <input className="input" value={trackingValue} onChange={(e) => setTracking(e.target.value)} />
                <button className="btn-outline px-3" onClick={() => update.mutate({ status: o.status, trackingCode: trackingValue })}><Check size={18} /></button>
              </div>
            </Field>
          </div>
          <div className="card space-y-2 p-5 text-sm">
            <h2 className="mb-2 font-bold">مشتری و ارسال</h2>
            <div><span className="text-muted">مشتری: </span>{o.customerName} ({toFaDigits(o.customerPhone ?? '')})</div>
            <div><span className="text-muted">گیرنده: </span>{o.receiverName} • {toFaDigits(o.receiverPhone)}</div>
            <div className="leading-7"><span className="text-muted">آدرس: </span>{o.province}، {o.city}، {o.fullAddress}</div>
            <div><span className="text-muted">کد پستی: </span>{toFaDigits(o.postalCode)}</div>
          </div>
        </aside>
      </div>
    </div>
  )
}

// ---------------- Users ----------------

export function UsersPage() {
  const { params, set, page } = useListParams()
  const me = useAuthStore((s) => s.user)
  const qc = useQueryClient()
  const query = { q: params.get('q') || undefined, role: params.get('role') || undefined, page, pageSize: 20 }
  const { data, isLoading } = useQuery({ queryKey: ['admin', 'users', query], queryFn: () => get<PagedResult<AdminUser>>('/admin/users', query), placeholderData: keepPreviousData })
  const update = useMutation({
    mutationFn: ({ id, role, isActive }: { id: number; role: UserRole; isActive: boolean }) => put(`/admin/users/${id}`, { role, isActive }),
    onSuccess: () => { toast.success('کاربر به‌روز شد'); qc.invalidateQueries({ queryKey: ['admin', 'users'] }) },
    onError: (e) => toast.error(errorMessage(e)),
  })
  return (
    <div>
      <PageHeader title="کاربران" subtitle={data ? `${formatNumber(data.totalCount)} کاربر` : undefined} />
      <div className="card mb-4 flex flex-wrap gap-3 p-4">
        <SearchInput initial={params.get('q') ?? ''} onSearch={(v) => set('q', v)} placeholder="نام، موبایل یا ایمیل..." />
        <select className="input w-auto" value={query.role ?? ''} onChange={(e) => set('role', e.target.value || null)}>
          <option value="">همه نقش‌ها</option><option value="Customer">مشتری</option><option value="Admin">مدیر</option>
        </select>
      </div>
      <div className="card overflow-x-auto">
        <table className="w-full min-w-[860px] text-sm">
          <thead className="bg-surface text-xs text-muted"><tr><th className="p-3 text-right">کاربر</th><th className="p-3 text-right">ایمیل</th><th className="p-3 text-right">سفارش موفق</th><th className="p-3 text-right">مجموع خرید</th><th className="p-3 text-right">عضویت</th><th className="p-3 text-right">نقش</th><th className="p-3 text-right">فعال</th></tr></thead>
          <tbody className="divide-y divide-line">
            {data?.items.map((u) => (
              <tr key={u.id} className="hover:bg-surface/50">
                <td className="p-3 font-bold">{u.fullName}<div className="text-xs font-normal text-muted" dir="ltr">{toFaDigits(u.phoneNumber)}</div></td>
                <td className="p-3 text-xs" dir="ltr">{u.email}</td>
                <td className="p-3">{toFaDigits(u.ordersCount)}</td>
                <td className="p-3">{formatNumber(u.totalSpent)}</td>
                <td className="p-3 text-xs text-muted">{formatDate(u.createdAt)}</td>
                <td className="p-3">
                  <select className="rounded-lg border border-line px-2 py-1 text-xs" value={u.role} disabled={u.id === me?.id}
                    onChange={(e) => update.mutate({ id: u.id, role: e.target.value as UserRole, isActive: u.isActive })}>
                    <option value="Customer">مشتری</option><option value="Admin">مدیر</option>
                  </select>
                </td>
                <td className="p-3">{u.id !== me?.id && <Toggle checked={u.isActive} onChange={(v) => update.mutate({ id: u.id, role: u.role, isActive: v })} />}</td>
              </tr>
            ))}
          </tbody>
        </table>
        {isLoading && <div className="p-10 text-center"><Spinner /></div>}
      </div>
      {data && <Pagination page={data.page} totalPages={data.totalPages} onChange={(p) => set('page', String(p))} />}
    </div>
  )
}

// ---------------- Reviews ----------------

export function ReviewsPage() {
  const { params, set, page } = useListParams()
  const qc = useQueryClient()
  const status = params.get('status') ?? 'pending'
  const query = { status: status === 'all' ? undefined : status, page, pageSize: 20 }
  const { data, isLoading } = useQuery({ queryKey: ['admin', 'reviews', query], queryFn: () => get<PagedResult<AdminReview>>('/admin/reviews', query), placeholderData: keepPreviousData })
  const act = useMutation({
    mutationFn: ({ id, action }: { id: number; action: 'approve' | 'reject' | 'delete' }) =>
      action === 'delete' ? del(`/admin/reviews/${id}`) : put(`/admin/reviews/${id}/${action}`),
    onSuccess: () => { qc.invalidateQueries({ queryKey: ['admin', 'reviews'] }); qc.invalidateQueries({ queryKey: ['admin', 'dashboard'] }) },
    onError: (e) => toast.error(errorMessage(e)),
  })
  return (
    <div>
      <PageHeader title="نظرات کاربران" />
      <div className="mb-4 flex gap-2">
        {[['pending', 'در انتظار تایید'], ['approved', 'تایید شده'], ['all', 'همه']].map(([k, l]) => (
          <button key={k} onClick={() => set('status', k)} className={clsx('chip py-1.5', status === k && 'border-brand-500 bg-brand-50 text-brand-600')}>{l}</button>
        ))}
      </div>
      {isLoading ? <PageLoader /> : data?.items.length === 0 ? <div className="card"><EmptyState title="نظری برای نمایش وجود ندارد" /></div> : (
        <div className="space-y-3">
          {data?.items.map((r) => (
            <div key={r.id} className="card p-4">
              <div className="mb-2 flex flex-wrap items-center gap-3">
                <RatingStars value={r.rating} />
                <b>{r.title}</b>
                <span className={clsx('rounded-full px-2 py-0.5 text-xs', r.isApproved ? 'bg-emerald-50 text-emerald-700' : 'bg-amber-50 text-amber-700')}>{r.isApproved ? 'تایید شده' : 'در انتظار'}</span>
                <div className="mr-auto flex gap-1">
                  {!r.isApproved && <button className="btn py-1.5 text-emerald-700 hover:bg-emerald-50" onClick={() => act.mutate({ id: r.id, action: 'approve' })}><Check size={16} /> تایید</button>}
                  {r.isApproved && <button className="btn py-1.5 text-amber-700 hover:bg-amber-50" onClick={() => act.mutate({ id: r.id, action: 'reject' })}><X size={16} /> عدم نمایش</button>}
                  <button className="btn py-1.5 text-rose-600 hover:bg-rose-50" onClick={() => confirm('نظر حذف شود؟') && act.mutate({ id: r.id, action: 'delete' })}><Trash2 size={16} /></button>
                </div>
              </div>
              <p className="mb-2 text-sm leading-7">{r.comment}</p>
              <div className="text-xs text-muted">{r.userName} • {formatDate(r.createdAt)} • <Link to={`/product/${r.productSlug}`} className="text-sky-600">{r.productTitle}</Link></div>
            </div>
          ))}
        </div>
      )}
      {data && <Pagination page={data.page} totalPages={data.totalPages} onChange={(p) => set('page', String(p))} />}
    </div>
  )
}

// ---------------- Coupons ----------------

type CouponForm = { code: string; type: CouponType; value: number; maxDiscountAmount: number | null; minOrderAmount: number; expiresAt: string; usageLimit: number | null; isActive: boolean }
const emptyCoupon: CouponForm = { code: '', type: 'Percent', value: 10, maxDiscountAmount: null, minOrderAmount: 0, expiresAt: '', usageLimit: null, isActive: true }

export function CouponsPage() {
  const qc = useQueryClient()
  const { data = [], isLoading } = useQuery({ queryKey: ['admin', 'coupons'], queryFn: () => get<Coupon[]>('/admin/coupons') })
  const [editing, setEditing] = useState<{ id?: number; form: CouponForm } | null>(null)
  const save = useMutation({
    mutationFn: () => {
      const f = editing!.form
      const body = { ...f, expiresAt: f.expiresAt ? new Date(f.expiresAt).toISOString() : null }
      return editing!.id ? put(`/admin/coupons/${editing!.id}`, body) : post('/admin/coupons', body)
    },
    onSuccess: () => { toast.success('ذخیره شد'); setEditing(null); qc.invalidateQueries({ queryKey: ['admin', 'coupons'] }) },
    onError: (e) => toast.error(errorMessage(e)),
  })
  const remove = useMutation({
    mutationFn: (id: number) => del(`/admin/coupons/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['admin', 'coupons'] }),
    onError: (e) => toast.error(errorMessage(e)),
  })
  if (isLoading) return <PageLoader />
  const f = editing?.form
  const setF = (patch: Partial<CouponForm>) => setEditing((e) => e && { ...e, form: { ...e.form, ...patch } })

  return (
    <div>
      <PageHeader title="کدهای تخفیف" actions={<button className="btn-primary" onClick={() => setEditing({ form: { ...emptyCoupon } })}><Plus size={18} /> کد جدید</button>} />
      <div className="card overflow-x-auto">
        <table className="w-full min-w-[760px] text-sm">
          <thead className="bg-surface text-xs text-muted"><tr><th className="p-3 text-right">کد</th><th className="p-3 text-right">تخفیف</th><th className="p-3 text-right">حداقل خرید</th><th className="p-3 text-right">استفاده</th><th className="p-3 text-right">انقضا</th><th className="p-3 text-right">وضعیت</th><th className="p-3" /></tr></thead>
          <tbody className="divide-y divide-line">
            {data.map((c) => {
              const expired = !!c.expiresAt && new Date(c.expiresAt) < new Date()
              return (
                <tr key={c.id} className="hover:bg-surface/50">
                  <td className="p-3 font-mono font-bold" dir="ltr">{c.code}</td>
                  <td className="p-3">{c.type === 'Percent' ? `${toFaDigits(c.value)}٪` : `${formatNumber(c.value)} تومان`}{c.maxDiscountAmount ? <div className="text-xs text-muted">سقف {formatNumber(c.maxDiscountAmount)}</div> : null}</td>
                  <td className="p-3">{formatNumber(c.minOrderAmount)}</td>
                  <td className="p-3">{toFaDigits(c.usedCount)}{c.usageLimit ? ` / ${toFaDigits(c.usageLimit)}` : ''}</td>
                  <td className="p-3 text-xs">{c.expiresAt ? formatDate(c.expiresAt) : '—'}</td>
                  <td className="p-3"><span className={clsx('rounded-full px-2 py-1 text-xs', expired ? 'bg-rose-50 text-rose-600' : c.isActive ? 'bg-emerald-50 text-emerald-700' : 'bg-gray-100 text-gray-500')}>{expired ? 'منقضی' : c.isActive ? 'فعال' : 'غیرفعال'}</span></td>
                  <td className="p-3"><div className="flex justify-end gap-1">
                    <button className="rounded-lg p-2 text-muted hover:bg-surface" onClick={() => setEditing({ id: c.id, form: { code: c.code, type: c.type, value: c.value, maxDiscountAmount: c.maxDiscountAmount ?? null, minOrderAmount: c.minOrderAmount, expiresAt: c.expiresAt ? c.expiresAt.slice(0, 10) : '', usageLimit: c.usageLimit ?? null, isActive: c.isActive } })}><Pencil size={17} /></button>
                    <button className="rounded-lg p-2 text-muted hover:bg-rose-50 hover:text-rose-600" onClick={() => confirm('کد حذف شود؟') && remove.mutate(c.id)}><Trash2 size={17} /></button>
                  </div></td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>
      <Modal open={!!editing} onClose={() => setEditing(null)} title={editing?.id ? 'ویرایش کد تخفیف' : 'کد تخفیف جدید'}>
        {f && (
          <form className="grid grid-cols-2 gap-4" onSubmit={(e) => { e.preventDefault(); save.mutate() }}>
            <Field label="کد (انگلیسی)" className="col-span-2"><input className="input text-left font-mono uppercase" dir="ltr" value={f.code} onChange={(e) => setF({ code: e.target.value })} required /></Field>
            <Field label="نوع">
              <select className="input" value={f.type} onChange={(e) => setF({ type: e.target.value as CouponType })}><option value="Percent">درصدی</option><option value="Fixed">مبلغ ثابت</option></select>
            </Field>
            <Field label={f.type === 'Percent' ? 'درصد' : 'مبلغ (تومان)'}><input className="input" type="number" min={1} value={f.value} onChange={(e) => setF({ value: Number(e.target.value) })} required /></Field>
            <Field label="سقف تخفیف (اختیاری)"><input className="input" type="number" value={f.maxDiscountAmount ?? ''} onChange={(e) => setF({ maxDiscountAmount: e.target.value ? Number(e.target.value) : null })} /></Field>
            <Field label="حداقل مبلغ سفارش"><input className="input" type="number" min={0} value={f.minOrderAmount} onChange={(e) => setF({ minOrderAmount: Number(e.target.value) })} /></Field>
            <Field label="تاریخ انقضا (اختیاری)"><input className="input" type="date" dir="ltr" value={f.expiresAt} onChange={(e) => setF({ expiresAt: e.target.value })} /></Field>
            <Field label="سقف تعداد استفاده"><input className="input" type="number" value={f.usageLimit ?? ''} onChange={(e) => setF({ usageLimit: e.target.value ? Number(e.target.value) : null })} /></Field>
            <div className="col-span-2"><Toggle label="فعال" checked={f.isActive} onChange={(v) => setF({ isActive: v })} /></div>
            <button className="btn-primary col-span-2" disabled={save.isPending}>ذخیره</button>
          </form>
        )}
      </Modal>
    </div>
  )
}
