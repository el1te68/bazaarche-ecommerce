import { useQueryClient } from '@tanstack/react-query'
import { clsx } from 'clsx'
import { BadgePercent, ExternalLink, FolderTree, Image, LayoutDashboard, LogOut, Menu, MessageSquare, Package, ShoppingBag, Tags, Users, X } from 'lucide-react'
import { useEffect, useState, type ReactNode } from 'react'
import { Link, NavLink, Outlet, ScrollRestoration, useLocation, useNavigate } from 'react-router'
import { useAuthStore } from '@/stores/auth'

const nav = [
  { to: '/admin', label: 'داشبورد', icon: LayoutDashboard, end: true },
  { to: '/admin/orders', label: 'سفارش‌ها', icon: ShoppingBag },
  { to: '/admin/products', label: 'محصولات', icon: Package },
  { to: '/admin/categories', label: 'دسته‌بندی‌ها', icon: FolderTree },
  { to: '/admin/brands', label: 'برندها', icon: Tags },
  { to: '/admin/users', label: 'کاربران', icon: Users },
  { to: '/admin/reviews', label: 'نظرات', icon: MessageSquare },
  { to: '/admin/banners', label: 'بنرها', icon: Image },
  { to: '/admin/coupons', label: 'کدهای تخفیف', icon: BadgePercent },
]

export function PageHeader({ title, subtitle, actions }: { title: string; subtitle?: string; actions?: ReactNode }) {
  return (
    <div className="mb-5 flex flex-wrap items-center justify-between gap-3">
      <div>
        <h1 className="text-xl font-black">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-muted">{subtitle}</p>}
      </div>
      {actions && <div className="flex flex-wrap gap-2">{actions}</div>}
    </div>
  )
}

export default function AdminLayout() {
  const user = useAuthStore((s) => s.user)!
  const logout = useAuthStore((s) => s.logout)
  const qc = useQueryClient()
  const navigate = useNavigate()
  const location = useLocation()
  const [open, setOpen] = useState(false)
  useEffect(() => setOpen(false), [location.pathname])

  return (
    <div className="min-h-screen bg-slate-100">
      <aside className={clsx('fixed inset-y-0 right-0 z-40 flex w-64 flex-col bg-ink text-white transition-transform lg:translate-x-0', open ? 'translate-x-0' : 'translate-x-full')}>
        <div className="flex items-center justify-between px-5 py-5">
          <Link to="/admin" className="flex items-center gap-2 text-xl font-black">
            <span className="flex size-9 items-center justify-center rounded-xl bg-brand-500"><ShoppingBag size={20} /></span>
            پنل بازارچه
          </Link>
          <button className="lg:hidden" onClick={() => setOpen(false)}><X /></button>
        </div>
        <nav className="flex-1 space-y-1 overflow-y-auto px-3">
          {nav.map(({ to, label, icon: Icon, end }) => (
            <NavLink key={to} to={to} end={end}
              className={({ isActive }) => clsx('flex items-center gap-3 rounded-xl px-4 py-3 text-sm transition', isActive ? 'bg-brand-500 font-bold' : 'text-white/75 hover:bg-white/10')}>
              <Icon size={19} /> {label}
            </NavLink>
          ))}
        </nav>
        <div className="border-t border-white/10 p-4 text-sm">
          <div className="mb-3 text-white/70">{user.fullName}</div>
          <div className="flex gap-2">
            <Link to="/" className="flex flex-1 items-center justify-center gap-1 rounded-lg bg-white/10 py-2 hover:bg-white/20"><ExternalLink size={15} /> فروشگاه</Link>
            <button onClick={() => { logout(); qc.clear(); navigate('/') }} className="flex flex-1 items-center justify-center gap-1 rounded-lg bg-white/10 py-2 hover:bg-white/20"><LogOut size={15} /> خروج</button>
          </div>
        </div>
      </aside>
      {open && <div className="fixed inset-0 z-30 bg-black/40 lg:hidden" onClick={() => setOpen(false)} />}

      <div className="lg:mr-64">
        <header className="sticky top-0 z-20 flex items-center gap-3 border-b border-line bg-white px-4 py-3 lg:hidden">
          <button onClick={() => setOpen(true)}><Menu /></button>
          <span className="font-bold">پنل مدیریت</span>
        </header>
        <main className="p-4 md:p-6"><Outlet /></main>
      </div>
      <ScrollRestoration />
    </div>
  )
}
