import { clsx } from 'clsx'
import { Home, LayoutGrid, ShoppingCart, User } from 'lucide-react'
import { NavLink } from 'react-router'
import { toFaDigits } from '@/lib/format'
import { useCart } from '@/hooks/useCart'

export function MobileNav() {
  const { cart } = useCart()
  const items = [
    { to: '/', label: 'خانه', icon: Home, end: true },
    { to: '/categories', label: 'دسته‌بندی', icon: LayoutGrid },
    { to: '/cart', label: 'سبد خرید', icon: ShoppingCart, badge: cart.totalQuantity },
    { to: '/profile', label: 'بازارچه من', icon: User },
  ]
  return (
    <nav className="fixed inset-x-0 bottom-0 z-30 grid grid-cols-4 border-t border-line bg-white md:hidden">
      {items.map(({ to, label, icon: Icon, badge, end }) => (
        <NavLink key={to} to={to} end={end}
          className={({ isActive }) => clsx('relative flex flex-col items-center gap-1 py-2 text-[11px]', isActive ? 'text-ink font-bold' : 'text-muted')}>
          <Icon size={22} />
          {!!badge && (
            <span className="absolute right-1/2 top-1 translate-x-4 rounded-md bg-brand-500 px-1 text-[10px] font-bold text-white">{toFaDigits(badge)}</span>
          )}
          {label}
        </NavLink>
      ))}
    </nav>
  )
}
