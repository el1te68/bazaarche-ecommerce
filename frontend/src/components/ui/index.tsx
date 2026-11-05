import { clsx } from 'clsx'
import { ChevronLeft, ChevronRight, Loader2, Star, X } from 'lucide-react'
import { useEffect, useState, type ReactNode } from 'react'
import { Link } from 'react-router'
import { formatPrice, orderStatusColor, orderStatusLabel, toFaDigits } from '@/lib/format'
import type { CategoryLink, OrderStatus } from '@/lib/types'

export function Spinner({ className }: { className?: string }) {
  return <Loader2 className={clsx('animate-spin text-brand-500', className ?? 'size-6')} />
}

export function PageLoader() {
  return (
    <div className="flex min-h-[50vh] items-center justify-center">
      <Spinner className="size-9" />
    </div>
  )
}

export function Skeleton({ className }: { className?: string }) {
  return <div className={clsx('skeleton rounded-lg', className)} />
}

/** Toman price with the small "تومان" label. */
export function Price({ value, className, size = 'md' }: { value: number; className?: string; size?: 'sm' | 'md' | 'lg' }) {
  return (
    <span className={clsx('inline-flex items-baseline gap-1 font-bold', className)}>
      <span className={clsx(size === 'lg' ? 'text-xl' : size === 'sm' ? 'text-sm' : 'text-base')}>{formatPrice(value)}</span>
      <span className="text-[10px] font-normal text-muted">تومان</span>
    </span>
  )
}

export function DiscountBadge({ percent, className }: { percent: number; className?: string }) {
  if (percent <= 0) return null
  return (
    <span className={clsx('rounded-full bg-brand-500 px-2 py-0.5 text-xs font-bold text-white', className)}>
      {toFaDigits(percent)}٪
    </span>
  )
}

export function RatingStars({ value, size = 14 }: { value: number; size?: number }) {
  return (
    <span className="inline-flex items-center gap-0.5" dir="ltr">
      {[1, 2, 3, 4, 5].map((i) => (
        <Star key={i} size={size} className={i <= Math.round(value) ? 'fill-amber-400 text-amber-400' : 'text-gray-300'} />
      ))}
    </span>
  )
}

export function RatingBadge({ value, count }: { value: number; count?: number }) {
  if (!value) return null
  return (
    <span className="inline-flex items-center gap-1 text-xs text-ink/80">
      <Star size={13} className="fill-amber-400 text-amber-400" />
      {toFaDigits(value.toFixed(1))}
      {count !== undefined && <span className="text-muted">({toFaDigits(count)})</span>}
    </span>
  )
}

function remaining(target: string) {
  const diff = Math.max(0, new Date(target).getTime() - Date.now())
  const h = Math.floor(diff / 3_600_000)
  const m = Math.floor((diff % 3_600_000) / 60_000)
  const s = Math.floor((diff % 60_000) / 1000)
  return [h, m, s].map((x) => String(x).padStart(2, '0'))
}

export function Countdown({ to, variant = 'light' }: { to: string; variant?: 'light' | 'dark' }) {
  const [parts, setParts] = useState(() => remaining(to))
  useEffect(() => {
    const id = setInterval(() => setParts(remaining(to)), 1000)
    return () => clearInterval(id)
  }, [to])
  return (
    <span className="inline-flex items-center gap-1 font-bold" dir="ltr">
      {parts.map((p, i) => (
        <span key={i} className="flex items-center gap-1">
          <span className={clsx('min-w-7 rounded-md px-1 py-0.5 text-center text-sm',
            variant === 'light' ? 'bg-white text-brand-600' : 'bg-brand-500 text-white')}>
            {toFaDigits(p)}
          </span>
          {i < 2 && <span className={variant === 'light' ? 'text-white' : 'text-brand-500'}>:</span>}
        </span>
      ))}
    </span>
  )
}

export function Pagination({ page, totalPages, onChange }: { page: number; totalPages: number; onChange: (p: number) => void }) {
  if (totalPages <= 1) return null
  const pages: (number | '…')[] = []
  for (let i = 1; i <= totalPages; i++) {
    if (i === 1 || i === totalPages || Math.abs(i - page) <= 1) pages.push(i)
    else if (pages[pages.length - 1] !== '…') pages.push('…')
  }
  return (
    <nav className="mt-6 flex items-center justify-center gap-1">
      <button className="btn-ghost px-2" disabled={page <= 1} onClick={() => onChange(page - 1)} aria-label="قبلی">
        <ChevronRight size={18} />
      </button>
      {pages.map((p, i) =>
        p === '…' ? (
          <span key={`e${i}`} className="px-2 text-muted">…</span>
        ) : (
          <button key={p} onClick={() => onChange(p)}
            className={clsx('size-9 rounded-full text-sm font-bold', p === page ? 'bg-brand-500 text-white' : 'hover:bg-surface')}>
            {toFaDigits(p)}
          </button>
        ),
      )}
      <button className="btn-ghost px-2" disabled={page >= totalPages} onClick={() => onChange(page + 1)} aria-label="بعدی">
        <ChevronLeft size={18} />
      </button>
    </nav>
  )
}

export function EmptyState({ icon, title, description, action }: { icon?: ReactNode; title: string; description?: string; action?: ReactNode }) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 px-4 py-14 text-center">
      {icon && <div className="text-gray-300">{icon}</div>}
      <h3 className="text-lg font-bold">{title}</h3>
      {description && <p className="max-w-md text-sm text-muted">{description}</p>}
      {action}
    </div>
  )
}

export function Modal({ open, onClose, title, children, wide }: { open: boolean; onClose: () => void; title: string; children: ReactNode; wide?: boolean }) {
  useEffect(() => {
    if (!open) return
    const onKey = (e: KeyboardEvent) => e.key === 'Escape' && onClose()
    document.addEventListener('keydown', onKey)
    document.body.style.overflow = 'hidden'
    return () => {
      document.removeEventListener('keydown', onKey)
      document.body.style.overflow = ''
    }
  }, [open, onClose])
  if (!open) return null
  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 p-0 sm:items-center sm:p-4" onMouseDown={onClose}>
      <div className={clsx('max-h-[92vh] w-full overflow-y-auto rounded-t-2xl bg-white shadow-xl sm:rounded-2xl', wide ? 'sm:max-w-3xl' : 'sm:max-w-lg')}
        onMouseDown={(e) => e.stopPropagation()}>
        <div className="sticky top-0 z-10 flex items-center justify-between border-b border-line bg-white px-5 py-4">
          <h3 className="font-bold">{title}</h3>
          <button onClick={onClose} className="rounded-full p-1 text-muted hover:bg-surface" aria-label="بستن"><X size={20} /></button>
        </div>
        <div className="p-5">{children}</div>
      </div>
    </div>
  )
}

export function StatusBadge({ status }: { status: OrderStatus }) {
  return (
    <span className={clsx('inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ring-1', orderStatusColor[status])}>
      {orderStatusLabel[status]}
    </span>
  )
}

export function Breadcrumbs({ items, current }: { items: CategoryLink[]; current?: string }) {
  return (
    <nav className="flex flex-wrap items-center gap-1 text-xs text-muted">
      <Link to="/" className="hover:text-ink">بازارچه</Link>
      {items.map((c) => (
        <span key={c.id} className="flex items-center gap-1">
          <span>/</span>
          <Link to={`/category/${c.slug}`} className="hover:text-ink">{c.name}</Link>
        </span>
      ))}
      {current && (
        <span className="flex items-center gap-1"><span>/</span><span className="line-clamp-1 text-ink/70">{current}</span></span>
      )}
    </nav>
  )
}

export function Field({ label, error, children, className }: { label: string; error?: string; children: ReactNode; className?: string }) {
  return (
    <label className={clsx('block', className)}>
      <span className="label">{label}</span>
      {children}
      {error && <span className="mt-1 block text-xs text-brand-600">{error}</span>}
    </label>
  )
}

export function Toggle({ checked, onChange, label }: { checked: boolean; onChange: (v: boolean) => void; label?: string }) {
  return (
    <button type="button" onClick={() => onChange(!checked)} className="flex items-center gap-2 text-sm">
      <span className={clsx('relative h-6 w-11 rounded-full transition', checked ? 'bg-brand-500' : 'bg-gray-300')}>
        <span className={clsx('absolute top-1 size-4 rounded-full bg-white shadow transition-all', checked ? 'right-6' : 'right-1')} />
      </span>
      {label}
    </button>
  )
}
