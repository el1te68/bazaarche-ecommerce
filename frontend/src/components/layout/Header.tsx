import { useQuery } from '@tanstack/react-query'
import { ChevronDown, ChevronLeft, Flame, LogIn, LogOut, Menu, Package, Percent, ShieldCheck, ShoppingCart, Sparkles, TrendingUp, User } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { Link, NavLink, useLocation, useNavigate } from 'react-router'
import { useQueryClient } from '@tanstack/react-query'
import { get } from '@/lib/api'
import { toFaDigits } from '@/lib/format'
import type { CategoryNode } from '@/lib/types'
import { useCart } from '@/hooks/useCart'
import { useAuthStore } from '@/stores/auth'
import { CategoryIcon } from '@/components/ui/CategoryIcon'
import { Logo } from './Logo'
import { SearchBox } from './SearchBox'

export function useCategories() {
  return useQuery({ queryKey: ['categories'], queryFn: () => get<CategoryNode[]>('/categories'), staleTime: 5 * 60_000 })
}

function MegaMenu({ onClose }: { onClose: () => void }) {
  const { data: categories = [] } = useCategories()
  const [active, setActive] = useState(0)
  const current = categories[active]
  return (
    <div className="absolute right-0 top-full z-40 flex w-[900px] max-w-[calc(100vw-2rem)] overflow-hidden rounded-b-xl border border-line bg-white shadow-xl">
      <ul className="w-56 shrink-0 border-l border-line bg-surface/50 py-2">
        {categories.map((c, i) => (
          <li key={c.id} onMouseEnter={() => setActive(i)}>
            <Link to={`/category/${c.slug}`} onClick={onClose}
              className={`flex items-center gap-2 px-4 py-2.5 text-sm ${i === active ? 'bg-white font-bold text-brand-500' : 'text-ink/80'}`}>
              <CategoryIcon name={c.icon} size={18} /> {c.name}
            </Link>
          </li>
        ))}
      </ul>
      {current && (
        <div className="flex-1 p-5">
          <Link to={`/category/${current.slug}`} onClick={onClose} className="mb-4 flex items-center gap-1 text-sm font-medium text-sky-600">
            همه محصولات {current.name} <ChevronLeft size={16} />
          </Link>
          <div className="grid grid-cols-3 gap-x-6 gap-y-3">
            {current.children.map((child) => (
              <Link key={child.id} to={`/category/${child.slug}`} onClick={onClose}
                className="flex items-center gap-2 border-r-2 border-brand-500 pr-2 text-sm font-bold hover:text-brand-500">
                {child.name}
              </Link>
            ))}
            {current.children.length === 0 && <p className="text-sm text-muted">زیرمجموعه‌ای ندارد.</p>}
          </div>
        </div>
      )}
    </div>
  )
}

function AccountMenu() {
  const user = useAuthStore((s) => s.user)
  const logout = useAuthStore((s) => s.logout)
  const [open, setOpen] = useState(false)
  const ref = useRef<HTMLDivElement>(null)
  const navigate = useNavigate()
  const qc = useQueryClient()

  useEffect(() => {
    const onClick = (e: MouseEvent) => !ref.current?.contains(e.target as Node) && setOpen(false)
    document.addEventListener('mousedown', onClick)
    return () => document.removeEventListener('mousedown', onClick)
  }, [])

  if (!user) {
    return (
      <Link to="/login" className="flex items-center gap-2 rounded-lg border border-line px-3 py-2 text-sm font-bold hover:bg-surface">
        <LogIn size={18} /> <span className="hidden sm:inline">ورود | ثبت‌نام</span>
      </Link>
    )
  }

  return (
    <div ref={ref} className="relative">
      <button onClick={() => setOpen(!open)} className="flex items-center gap-1 rounded-lg px-2 py-2 hover:bg-surface">
        <User size={22} /> <ChevronDown size={14} />
      </button>
      {open && (
        <div className="absolute left-0 top-full z-40 mt-1 w-64 rounded-xl border border-line bg-white py-2 shadow-xl">
          <Link to="/profile" onClick={() => setOpen(false)} className="flex items-center justify-between px-4 py-3 hover:bg-surface">
            <span className="font-bold">{user.fullName}</span> <ChevronLeft size={16} />
          </Link>
          {user.role === 'Admin' && (
            <Link to="/admin" onClick={() => setOpen(false)} className="flex items-center gap-2 px-4 py-2.5 text-sm text-brand-600 hover:bg-surface">
              <ShieldCheck size={18} /> پنل مدیریت
            </Link>
          )}
          <Link to="/profile/orders" onClick={() => setOpen(false)} className="flex items-center gap-2 px-4 py-2.5 text-sm hover:bg-surface">
            <Package size={18} /> سفارش‌ها
          </Link>
          <button onClick={() => { logout(); qc.clear(); setOpen(false); navigate('/') }}
            className="flex w-full items-center gap-2 px-4 py-2.5 text-sm hover:bg-surface">
            <LogOut size={18} /> خروج از حساب
          </button>
        </div>
      )}
    </div>
  )
}

export function Header() {
  const { cart } = useCart()
  const [menuOpen, setMenuOpen] = useState(false)
  const location = useLocation()
  useEffect(() => setMenuOpen(false), [location.pathname])

  return (
    <header className="sticky top-0 z-30 border-b border-line bg-white">
      <div className="container-x flex items-center gap-4 py-3">
        <Logo />
        <div className="hidden flex-1 md:block"><SearchBox /></div>
        <div className="mr-auto flex items-center gap-2">
          <AccountMenu />
          <span className="hidden h-6 w-px bg-line sm:block" />
          <Link to="/cart" className="relative rounded-lg p-2 hover:bg-surface" aria-label="سبد خرید">
            <ShoppingCart size={24} />
            {cart.totalQuantity > 0 && (
              <span className="absolute -bottom-0.5 -right-0.5 flex size-5 items-center justify-center rounded-md border-2 border-white bg-brand-500 text-[10px] font-bold text-white">
                {toFaDigits(cart.totalQuantity)}
              </span>
            )}
          </Link>
        </div>
      </div>
      <div className="container-x pb-3 md:hidden"><SearchBox /></div>
      <nav className="container-x hidden items-center gap-6 pb-2 text-sm text-ink/80 md:flex">
        <div className="relative" onMouseEnter={() => setMenuOpen(true)} onMouseLeave={() => setMenuOpen(false)}>
          <button className="flex items-center gap-2 py-2 font-bold text-ink"><Menu size={18} /> دسته‌بندی کالاها</button>
          {menuOpen && <MegaMenu onClose={() => setMenuOpen(false)} />}
        </div>
        <span className="h-4 w-px bg-line" />
        <NavLink to="/search?amazing=true" className="flex items-center gap-1 hover:text-ink"><Percent size={17} /> شگفت‌انگیزها</NavLink>
        <NavLink to="/search?sort=bestselling" className="flex items-center gap-1 hover:text-ink"><TrendingUp size={17} /> پرفروش‌ترین‌ها</NavLink>
        <NavLink to="/search?discount=true&sort=discount" className="flex items-center gap-1 hover:text-ink"><Flame size={17} /> تخفیف‌ها و پیشنهادها</NavLink>
        <NavLink to="/search?sort=newest" className="flex items-center gap-1 hover:text-ink"><Sparkles size={17} /> جدیدترین‌ها</NavLink>
      </nav>
    </header>
  )
}
