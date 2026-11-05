import { ShoppingBag } from 'lucide-react'
import { Link } from 'react-router'

export function Logo({ light }: { light?: boolean }) {
  return (
    <Link to="/" className="flex shrink-0 items-center gap-1.5" aria-label="بازارچه">
      <span className="flex size-9 items-center justify-center rounded-xl bg-brand-500 text-white shadow-sm">
        <ShoppingBag size={20} strokeWidth={2.4} />
      </span>
      <span className={`text-2xl font-black tracking-tight ${light ? 'text-white' : 'text-brand-500'}`}>بازارچه</span>
    </Link>
  )
}
