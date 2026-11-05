import { useQuery } from '@tanstack/react-query'
import { AlertTriangle, MessageSquare, Package, ShoppingBag, TrendingUp, Users, Wallet } from 'lucide-react'
import { Link } from 'react-router'
import { Area, AreaChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { get } from '@/lib/api'
import { formatDateTime, formatNumber, formatShortDate, img, orderStatusLabel, toFaDigits } from '@/lib/format'
import type { Dashboard, OrderStatus } from '@/lib/types'
import { PageLoader, StatusBadge } from '@/components/ui'
import { PageHeader } from './AdminLayout'

const statusColors: Record<OrderStatus, string> = {
  PendingPayment: '#f59e0b', Processing: '#0ea5e9', Shipped: '#6366f1', Delivered: '#10b981', Cancelled: '#f43f5e',
}

function Stat({ icon: Icon, label, value, hint, color }: { icon: typeof Wallet; label: string; value: string; hint?: string; color: string }) {
  return (
    <div className="card flex items-center gap-4 p-5">
      <span className={`flex size-12 items-center justify-center rounded-2xl ${color}`}><Icon size={24} /></span>
      <div>
        <div className="text-xs text-muted">{label}</div>
        <div className="text-xl font-black">{value}</div>
        {hint && <div className="text-xs text-muted">{hint}</div>}
      </div>
    </div>
  )
}

const compact = (n: number) => n >= 1_000_000_000 ? `${toFaDigits((n / 1_000_000_000).toFixed(1))} میلیارد` : n >= 1_000_000 ? `${toFaDigits(Math.round(n / 1_000_000))} م` : formatNumber(n)

export default function DashboardPage() {
  const { data, isLoading } = useQuery({ queryKey: ['admin', 'dashboard'], queryFn: () => get<Dashboard>('/admin/dashboard') })
  if (isLoading || !data) return <PageLoader />
  const totalStatus = data.orderStatuses.reduce((s, x) => s + x.count, 0)
  const chart = data.sales.map((s) => ({ ...s, label: formatShortDate(s.date) }))

  return (
    <div className="space-y-5">
      <PageHeader title="داشبورد" subtitle="نمای کلی فروشگاه در یک نگاه" />
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <Stat icon={Wallet} label="درآمد کل (تومان)" value={compact(data.totalRevenue)} hint={`امروز: ${formatNumber(data.todayRevenue)}`} color="bg-emerald-50 text-emerald-600" />
        <Stat icon={ShoppingBag} label="سفارش‌های موفق" value={formatNumber(data.totalOrders)} hint={`امروز: ${toFaDigits(data.todayOrders)}`} color="bg-sky-50 text-sky-600" />
        <Stat icon={Users} label="مشتریان" value={formatNumber(data.totalCustomers)} color="bg-violet-50 text-violet-600" />
        <Stat icon={Package} label="محصولات" value={formatNumber(data.totalProducts)} color="bg-amber-50 text-amber-600" />
      </div>

      {data.pendingReviews > 0 && (
        <Link to="/admin/reviews" className="flex items-center gap-3 rounded-xl bg-amber-50 p-4 text-sm text-amber-800">
          <MessageSquare size={20} /> {toFaDigits(data.pendingReviews)} نظر در انتظار تایید است. برای بررسی کلیک کنید.
        </Link>
      )}

      <div className="grid gap-4 xl:grid-cols-[1fr_320px]">
        <div className="card p-5">
          <h2 className="mb-4 flex items-center gap-2 font-bold"><TrendingUp size={20} className="text-brand-500" /> فروش ۳۰ روز اخیر</h2>
          <div className="h-72" dir="ltr">
            <ResponsiveContainer width="100%" height="100%">
              <AreaChart data={chart} margin={{ left: 10, right: 10, top: 10 }}>
                <defs>
                  <linearGradient id="rev" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="#ef4056" stopOpacity={0.35} />
                    <stop offset="100%" stopColor="#ef4056" stopOpacity={0} />
                  </linearGradient>
                </defs>
                <CartesianGrid strokeDasharray="3 3" stroke="#eee" vertical={false} />
                <XAxis dataKey="label" tick={{ fontSize: 11, fontFamily: 'Vazirmatn' }} interval={4} reversed />
                <YAxis tickFormatter={compact} tick={{ fontSize: 11, fontFamily: 'Vazirmatn' }} width={70} orientation="right" />
                <Tooltip formatter={(v) => [`${formatNumber(Number(v))} تومان`, 'فروش']} labelStyle={{ fontFamily: 'Vazirmatn' }} contentStyle={{ fontFamily: 'Vazirmatn', direction: 'rtl' }} />
                <Area type="monotone" dataKey="revenue" stroke="#ef4056" strokeWidth={2.5} fill="url(#rev)" />
              </AreaChart>
            </ResponsiveContainer>
          </div>
        </div>
        <div className="card p-5">
          <h2 className="mb-4 font-bold">وضعیت سفارش‌ها</h2>
          <div className="mb-4 flex h-3 overflow-hidden rounded-full bg-surface">
            {data.orderStatuses.map((s) => (
              <div key={s.status} style={{ width: `${(s.count / Math.max(totalStatus, 1)) * 100}%`, background: statusColors[s.status] }} />
            ))}
          </div>
          <ul className="space-y-3 text-sm">
            {data.orderStatuses.map((s) => (
              <li key={s.status} className="flex items-center gap-2">
                <span className="size-3 rounded-full" style={{ background: statusColors[s.status] }} />
                <span className="flex-1">{orderStatusLabel[s.status]}</span>
                <b>{toFaDigits(s.count)}</b>
              </li>
            ))}
          </ul>
        </div>
      </div>

      <div className="grid gap-4 xl:grid-cols-3">
        <div className="card h-fit overflow-hidden xl:col-span-2">
          <div className="flex items-center justify-between p-5 pb-3"><h2 className="font-bold">آخرین سفارش‌ها</h2><Link to="/admin/orders" className="text-sm text-sky-600">همه سفارش‌ها</Link></div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-surface text-xs text-muted"><tr><th className="p-3 text-right">سفارش</th><th className="p-3 text-right">مشتری</th><th className="p-3 text-right">مبلغ</th><th className="p-3 text-right">وضعیت</th><th className="p-3 text-right">زمان</th></tr></thead>
              <tbody className="divide-y divide-line">
                {data.recentOrders.map((o) => (
                  <tr key={o.id} className="hover:bg-surface/50">
                    <td className="p-3"><Link to={`/admin/orders/${o.id}`} className="font-bold text-sky-600" dir="ltr">{o.orderNumber}</Link></td>
                    <td className="p-3">{o.customerName}</td>
                    <td className="p-3">{formatNumber(o.amount)}</td>
                    <td className="p-3"><StatusBadge status={o.status} /></td>
                    <td className="p-3 text-xs text-muted">{formatDateTime(o.createdAt)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
        <div className="space-y-4">
          <div className="card p-5">
            <h2 className="mb-3 flex items-center gap-2 font-bold"><AlertTriangle size={18} className="text-amber-500" /> موجودی رو به اتمام</h2>
            <ul className="space-y-3">
              {data.lowStock.map((p) => (
                <li key={p.id}><Link to={`/admin/products/${p.id}`} className="flex items-center gap-3 text-sm">
                  <img src={img(p.imageUrl)} alt="" className="size-10 rounded object-contain" />
                  <span className="line-clamp-1 flex-1">{p.title}</span>
                  <span className={`rounded-md px-2 py-0.5 text-xs font-bold ${p.stock === 0 ? 'bg-rose-50 text-rose-600' : 'bg-amber-50 text-amber-700'}`}>{p.stock === 0 ? 'ناموجود' : toFaDigits(p.stock)}</span>
                </Link></li>
              ))}
              {data.lowStock.length === 0 && <li className="text-sm text-muted">همه محصولات موجودی کافی دارند.</li>}
            </ul>
          </div>
          <div className="card p-5">
            <h2 className="mb-3 font-bold">پرفروش‌ترین‌ها</h2>
            <ul className="space-y-3">
              {data.topProducts.map((p, i) => (
                <li key={p.id} className="flex items-center gap-3 text-sm">
                  <span className="w-4 font-black text-muted">{toFaDigits(i + 1)}</span>
                  <img src={img(p.imageUrl)} alt="" className="size-10 rounded object-contain" />
                  <span className="line-clamp-1 flex-1">{p.title}</span>
                  <b>{toFaDigits(p.soldCount)}</b>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </div>
  )
}
