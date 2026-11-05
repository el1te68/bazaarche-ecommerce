import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { clsx } from 'clsx'
import { ChevronLeft, Heart, Home, LogOut, MapPin, Package, Pencil, Plus, ShieldCheck, Trash2, UserRound } from 'lucide-react'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { Link, NavLink, Outlet, useNavigate, useParams, useSearchParams } from 'react-router'
import { del, errorMessage, get, post, put } from '@/lib/api'
import { formatDate, formatDateTime, formatNumber, img, orderStatusLabel, toFaDigits } from '@/lib/format'
import type { Address, CheckoutResult, OrderDetail, OrderStatus, OrderSummary, User } from '@/lib/types'
import { useWishlist } from '@/hooks/useWishlist'
import { useAuthStore } from '@/stores/auth'
import { EmptyState, Field, Modal, PageLoader, Price, StatusBadge } from '@/components/ui'
import { AddressForm } from '@/components/AddressForm'
import { ProductCard } from '@/components/product/ProductCard'

export function ProfileLayout() {
  const user = useAuthStore((s) => s.user)!
  const logout = useAuthStore((s) => s.logout)
  const qc = useQueryClient()
  const navigate = useNavigate()
  const links = [
    { to: '/profile', label: 'خلاصه فعالیت‌ها', icon: Home, end: true },
    { to: '/profile/orders', label: 'سفارش‌ها', icon: Package },
    { to: '/profile/wishlist', label: 'علاقه‌مندی‌ها', icon: Heart },
    { to: '/profile/addresses', label: 'آدرس‌ها', icon: MapPin },
    { to: '/profile/account', label: 'اطلاعات حساب کاربری', icon: UserRound },
  ]
  return (
    <div className="container-x grid gap-4 py-6 lg:grid-cols-[280px_1fr]">
      <aside className="card h-fit overflow-hidden">
        <div className="flex items-center gap-3 border-b border-line p-5">
          <span className="flex size-12 items-center justify-center rounded-full bg-surface"><UserRound className="text-muted" /></span>
          <div><div className="font-bold">{user.fullName}</div><div className="text-xs text-muted" dir="ltr">{toFaDigits(user.phoneNumber)}</div></div>
        </div>
        {user.role === 'Admin' && (
          <Link to="/admin" className="flex items-center gap-3 border-b border-line bg-brand-50 px-5 py-3 text-sm font-bold text-brand-600">
            <ShieldCheck size={20} /> ورود به پنل مدیریت
          </Link>
        )}
        <nav className="flex flex-col">
          {links.map(({ to, label, icon: Icon, end }) => (
            <NavLink key={to} to={to} end={end}
              className={({ isActive }) => clsx('flex items-center gap-3 border-r-4 px-5 py-3.5 text-sm', isActive ? 'border-brand-500 font-bold' : 'border-transparent text-ink/80 hover:bg-surface')}>
              <Icon size={20} /> {label}
            </NavLink>
          ))}
          <button onClick={() => { logout(); qc.clear(); navigate('/') }} className="flex items-center gap-3 border-r-4 border-transparent px-5 py-3.5 text-sm text-ink/80 hover:bg-surface">
            <LogOut size={20} /> خروج
          </button>
        </nav>
      </aside>
      <section className="min-w-0"><Outlet /></section>
    </div>
  )
}

const tabs: { key: OrderStatus; label: string }[] = [
  { key: 'PendingPayment', label: 'در انتظار پرداخت' },
  { key: 'Processing', label: 'جاری' },
  { key: 'Shipped', label: 'ارسال شده' },
  { key: 'Delivered', label: 'تحویل شده' },
  { key: 'Cancelled', label: 'لغو شده' },
]

function useOrders() {
  return useQuery({ queryKey: ['orders'], queryFn: () => get<OrderSummary[]>('/orders') })
}

export function ProfileHome() {
  const { data: orders = [], isLoading } = useOrders()
  if (isLoading) return <PageLoader />
  return (
    <div className="space-y-4">
      <div className="card p-5">
        <div className="mb-4 flex items-center justify-between"><h2 className="font-bold">سفارش‌های من</h2>
          <Link to="/profile/orders" className="flex items-center text-sm text-sky-600">مشاهده همه <ChevronLeft size={16} /></Link></div>
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-5">
          {tabs.map((t) => (
            <Link key={t.key} to={`/profile/orders?status=${t.key}`} className="rounded-xl bg-surface p-4 text-center hover:bg-brand-50">
              <div className="text-2xl font-black">{toFaDigits(orders.filter((o) => o.status === t.key).length)}</div>
              <div className="text-xs text-muted">{t.label}</div>
            </Link>
          ))}
        </div>
      </div>
      {orders.length > 0 && (
        <div className="card p-5">
          <h2 className="mb-4 font-bold">آخرین سفارش</h2>
          <OrderRow order={orders[0]} />
        </div>
      )}
    </div>
  )
}

function OrderRow({ order }: { order: OrderSummary }) {
  return (
    <Link to={`/profile/orders/${order.id}`} className="block rounded-xl border border-line p-4 transition hover:shadow-card">
      <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
        <StatusBadge status={order.status} />
        <ChevronLeft size={18} className="text-muted" />
      </div>
      <div className="mb-3 flex flex-wrap gap-x-6 gap-y-1 text-xs text-muted">
        <span>{formatDate(order.createdAt)}</span>
        <span>کد سفارش <b className="text-ink" dir="ltr">{order.orderNumber}</b></span>
        <span>مبلغ <b className="text-ink">{formatNumber(order.payableAmount)} تومان</b></span>
      </div>
      <div className="flex gap-2">
        {order.images.map((src, i) => <img key={i} src={img(src)} alt="" className="size-16 rounded-lg object-contain" />)}
      </div>
    </Link>
  )
}

export function OrdersPage() {
  const [params, setParams] = useSearchParams()
  const status = (params.get('status') as OrderStatus) ?? 'Processing'
  const { data: orders = [], isLoading } = useOrders()
  const filtered = orders.filter((o) => o.status === status)
  return (
    <div className="card p-5">
      <h1 className="mb-4 text-lg font-bold">تاریخچه سفارشات</h1>
      <div className="scrollbar-none mb-4 flex gap-1 overflow-x-auto border-b border-line">
        {tabs.map((t) => {
          const count = orders.filter((o) => o.status === t.key).length
          return (
            <button key={t.key} onClick={() => setParams({ status: t.key })}
              className={clsx('flex shrink-0 items-center gap-2 border-b-4 px-3 py-3 text-sm', status === t.key ? 'border-brand-500 font-bold text-brand-500' : 'border-transparent text-muted')}>
              {t.label}
              {count > 0 && <span className={clsx('rounded-md px-1.5 text-xs text-white', status === t.key ? 'bg-brand-500' : 'bg-gray-400')}>{toFaDigits(count)}</span>}
            </button>
          )
        })}
      </div>
      {isLoading ? <PageLoader /> : filtered.length === 0 ? (
        <EmptyState icon={<Package size={64} strokeWidth={1} />} title={`سفارش ${orderStatusLabel[status]} ندارید`} />
      ) : (
        <div className="space-y-3">{filtered.map((o) => <OrderRow key={o.id} order={o} />)}</div>
      )}
    </div>
  )
}

export function OrderDetailPage() {
  const { id } = useParams()
  const qc = useQueryClient()
  const navigate = useNavigate()
  const { data: o, isLoading } = useQuery({ queryKey: ['order', id], queryFn: () => get<OrderDetail>(`/orders/${id}`) })
  const pay = useMutation({
    mutationFn: () => post<CheckoutResult>(`/orders/${id}/pay`),
    onSuccess: (r) => navigate(`/payment/gateway/${r.paymentToken}`),
    onError: (e) => toast.error(errorMessage(e)),
  })
  const cancel = useMutation({
    mutationFn: () => post(`/orders/${id}/cancel`),
    onSuccess: () => { toast.success('سفارش لغو شد'); qc.invalidateQueries({ queryKey: ['order', id] }); qc.invalidateQueries({ queryKey: ['orders'] }) },
    onError: (e) => toast.error(errorMessage(e)),
  })
  if (isLoading || !o) return <PageLoader />
  const steps: OrderStatus[] = ['Processing', 'Shipped', 'Delivered']
  const current = steps.indexOf(o.status)

  return (
    <div className="space-y-4">
      <div className="card p-5">
        <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
          <h1 className="text-lg font-bold">جزئیات سفارش</h1>
          <StatusBadge status={o.status} />
        </div>
        <div className="grid gap-3 text-sm sm:grid-cols-2">
          <div><span className="text-muted">کد سفارش: </span><b dir="ltr">{o.orderNumber}</b></div>
          <div><span className="text-muted">تاریخ ثبت: </span>{formatDateTime(o.createdAt)}</div>
          <div><span className="text-muted">تحویل گیرنده: </span>{o.receiverName} • {toFaDigits(o.receiverPhone)}</div>
          <div><span className="text-muted">آدرس: </span>{o.province}، {o.city}، {o.fullAddress}</div>
          <div><span className="text-muted">مبلغ: </span><b>{formatNumber(o.payableAmount)} تومان</b></div>
          {o.trackingCode && <div><span className="text-muted">کد رهگیری مرسوله: </span><b>{toFaDigits(o.trackingCode)}</b></div>}
        </div>

        {o.status === 'PendingPayment' && (
          <div className="mt-5 flex flex-wrap items-center gap-3 rounded-xl bg-amber-50 p-4 text-sm text-amber-800">
            <span className="flex-1">این سفارش هنوز پرداخت نشده است. کالاهای سفارش برای شما رزرو شده‌اند.</span>
            <button className="btn-primary" onClick={() => pay.mutate()} disabled={pay.isPending}>پرداخت</button>
            <button className="btn-ghost" onClick={() => confirm('سفارش لغو شود؟') && cancel.mutate()} disabled={cancel.isPending}>لغو سفارش</button>
          </div>
        )}

        {current >= 0 && (
          <div className="mt-6 flex items-center">
            {steps.map((s, i) => (
              <div key={s} className="flex flex-1 items-center last:flex-none">
                <div className="flex flex-col items-center gap-1">
                  <span className={clsx('flex size-8 items-center justify-center rounded-full text-xs font-bold', i <= current ? 'bg-success text-white' : 'bg-surface text-muted')}>{toFaDigits(i + 1)}</span>
                  <span className="text-xs">{orderStatusLabel[s]}</span>
                </div>
                {i < steps.length - 1 && <div className={clsx('mx-2 mb-5 h-1 flex-1 rounded', i < current ? 'bg-success' : 'bg-surface')} />}
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="card p-5">
        <h2 className="mb-4 font-bold">کالاها</h2>
        <ul className="divide-y divide-line">
          {o.items.map((i) => (
            <li key={i.variantId} className="flex gap-4 py-4">
              <img src={img(i.imageUrl)} alt="" className="size-20 rounded-lg object-contain" />
              <div className="flex-1 space-y-1 text-sm">
                <Link to={`/product/${i.slug}`} className="line-clamp-2 font-bold">{i.title}</Link>
                {i.colorName && <div className="text-xs text-muted">رنگ: {i.colorName}</div>}
                <div className="text-xs text-muted">تعداد: {toFaDigits(i.quantity)}</div>
              </div>
              <Price value={i.unitFinalPrice * i.quantity} />
            </li>
          ))}
        </ul>
        <div className="mt-4 space-y-2 border-t border-line pt-4 text-sm">
          <div className="flex justify-between text-muted"><span>قیمت کالاها</span><span>{formatNumber(o.itemsTotal)}</span></div>
          {o.productsDiscount > 0 && <div className="flex justify-between text-brand-500"><span>تخفیف کالاها</span><span>{formatNumber(o.productsDiscount)}-</span></div>}
          {o.couponDiscount > 0 && <div className="flex justify-between text-brand-500"><span>کد تخفیف ({o.couponCode})</span><span>{formatNumber(o.couponDiscount)}-</span></div>}
          <div className="flex justify-between text-muted"><span>هزینه ارسال</span><span>{o.shippingCost ? formatNumber(o.shippingCost) : 'رایگان'}</span></div>
          <div className="flex justify-between font-bold"><span>مبلغ کل</span><Price value={o.payableAmount} /></div>
        </div>
      </div>

      {o.payments.length > 0 && (
        <div className="card p-5">
          <h2 className="mb-4 font-bold">تراکنش‌ها</h2>
          <ul className="space-y-2 text-sm">
            {o.payments.map((p) => (
              <li key={p.id} className="flex flex-wrap items-center justify-between gap-2 rounded-lg bg-surface p-3">
                <span>{formatDateTime(p.createdAt)}</span>
                <span className={p.status === 'Succeeded' ? 'text-success' : p.status === 'Failed' ? 'text-brand-500' : 'text-amber-600'}>
                  {p.status === 'Succeeded' ? 'موفق' : p.status === 'Failed' ? 'ناموفق' : 'در انتظار'}
                </span>
                {p.referenceId && <span className="text-muted">کد پیگیری: {toFaDigits(p.referenceId)}</span>}
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}

export function WishlistPage() {
  const { items, isLoading, toggle } = useWishlist()
  if (isLoading) return <PageLoader />
  return (
    <div className="card p-5">
      <h1 className="mb-4 text-lg font-bold">لیست علاقه‌مندی‌ها</h1>
      {items.length === 0 ? (
        <EmptyState icon={<Heart size={64} strokeWidth={1} />} title="لیست علاقه‌مندی‌های شما خالی است" />
      ) : (
        <div className="grid grid-cols-2 border-r border-t border-line md:grid-cols-3 xl:grid-cols-4">
          {items.map((p) => (
            <div key={p.id} className="relative">
              <ProductCard product={p} />
              <button onClick={() => toggle(p.id)} className="absolute left-2 top-2 rounded-full bg-white p-1.5 text-muted shadow-card hover:text-brand-500" aria-label="حذف"><Trash2 size={16} /></button>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

export function AddressesPage() {
  const user = useAuthStore((s) => s.user)!
  const qc = useQueryClient()
  const [modal, setModal] = useState<{ open: boolean; address?: Address }>({ open: false })
  const { data = [], isLoading } = useQuery({ queryKey: ['addresses'], queryFn: () => get<Address[]>('/addresses') })
  const remove = useMutation({
    mutationFn: (id: number) => del(`/addresses/${id}`),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['addresses'] }),
    onError: (e) => toast.error(errorMessage(e)),
  })
  return (
    <div className="card p-5">
      <div className="mb-4 flex items-center justify-between">
        <h1 className="text-lg font-bold">آدرس‌ها</h1>
        <button className="btn-outline py-2" onClick={() => setModal({ open: true })}><Plus size={18} /> ثبت آدرس جدید</button>
      </div>
      {isLoading ? <PageLoader /> : data.length === 0 ? (
        <EmptyState icon={<MapPin size={64} strokeWidth={1} />} title="هنوز آدرسی ثبت نکرده‌اید" />
      ) : (
        <ul className="divide-y divide-line">
          {data.map((a) => (
            <li key={a.id} className="flex gap-3 py-4">
              <div className="flex-1 space-y-2 text-sm">
                <div className="flex items-center gap-2 font-bold">{a.title} {a.isDefault && <span className="chip border-sky-200 bg-sky-50 text-sky-700">پیش‌فرض</span>}</div>
                <p className="leading-7">{a.fullAddress}</p>
                <p className="text-muted">{a.province}، {a.city} • کد پستی: {toFaDigits(a.postalCode)}</p>
                <p className="text-muted">{a.receiverName} • {toFaDigits(a.receiverPhone)}</p>
              </div>
              <div className="flex flex-col gap-2">
                <button className="rounded-lg p-2 text-muted hover:bg-surface" onClick={() => setModal({ open: true, address: a })} aria-label="ویرایش"><Pencil size={18} /></button>
                <button className="rounded-lg p-2 text-muted hover:bg-surface hover:text-brand-500" onClick={() => confirm('آدرس حذف شود؟') && remove.mutate(a.id)} aria-label="حذف"><Trash2 size={18} /></button>
              </div>
            </li>
          ))}
        </ul>
      )}
      <Modal open={modal.open} onClose={() => setModal({ open: false })} title={modal.address ? 'ویرایش آدرس' : 'آدرس جدید'} wide>
        <AddressForm address={modal.address} defaultName={user.fullName} defaultPhone={user.phoneNumber}
          onSaved={() => { qc.invalidateQueries({ queryKey: ['addresses'] }); setModal({ open: false }) }} />
      </Modal>
    </div>
  )
}

export function AccountPage() {
  const user = useAuthStore((s) => s.user)!
  const setUser = useAuthStore((s) => s.setUser)
  const [fullName, setFullName] = useState(user.fullName)
  const [email, setEmail] = useState(user.email ?? '')
  const [pw, setPw] = useState({ currentPassword: '', newPassword: '' })

  const profile = useMutation({
    mutationFn: () => put<User>('/auth/profile', { fullName, email: email || null }),
    onSuccess: (u) => { setUser(u); toast.success('اطلاعات حساب به‌روز شد') },
    onError: (e) => toast.error(errorMessage(e)),
  })
  const password = useMutation({
    mutationFn: () => put('/auth/password', pw),
    onSuccess: () => { setPw({ currentPassword: '', newPassword: '' }); toast.success('رمز عبور تغییر کرد') },
    onError: (e) => toast.error(errorMessage(e)),
  })

  return (
    <div className="space-y-4">
      <form className="card grid gap-4 p-5 sm:grid-cols-2" onSubmit={(e) => { e.preventDefault(); profile.mutate() }}>
        <h1 className="text-lg font-bold sm:col-span-2">اطلاعات حساب کاربری</h1>
        <Field label="نام و نام خانوادگی"><input className="input" value={fullName} onChange={(e) => setFullName(e.target.value)} required /></Field>
        <Field label="شماره موبایل"><input className="input bg-surface text-left" dir="ltr" value={user.phoneNumber} disabled /></Field>
        <Field label="ایمیل"><input className="input text-left" dir="ltr" type="email" value={email} onChange={(e) => setEmail(e.target.value)} /></Field>
        <Field label="عضویت از"><input className="input bg-surface" value={formatDate(user.createdAt)} disabled /></Field>
        <button className="btn-primary w-fit sm:col-span-2" disabled={profile.isPending}>ذخیره تغییرات</button>
      </form>
      <form className="card grid gap-4 p-5 sm:grid-cols-2" onSubmit={(e) => { e.preventDefault(); password.mutate() }}>
        <h2 className="font-bold sm:col-span-2">تغییر رمز عبور</h2>
        <Field label="رمز عبور فعلی"><input className="input" type="password" dir="ltr" value={pw.currentPassword} onChange={(e) => setPw({ ...pw, currentPassword: e.target.value })} required /></Field>
        <Field label="رمز عبور جدید"><input className="input" type="password" dir="ltr" value={pw.newPassword} onChange={(e) => setPw({ ...pw, newPassword: e.target.value })} required minLength={6} /></Field>
        <button className="btn-dark w-fit sm:col-span-2" disabled={password.isPending}>تغییر رمز</button>
      </form>
    </div>
  )
}
