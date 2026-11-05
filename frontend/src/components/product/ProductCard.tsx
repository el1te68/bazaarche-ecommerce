import { clsx } from 'clsx'
import { Link } from 'react-router'
import { img } from '@/lib/format'
import type { ProductCard as Product } from '@/lib/types'
import { DiscountBadge, Price, RatingBadge } from '@/components/ui'

export function ProductCard({ product, variant = 'grid' }: { product: Product; variant?: 'grid' | 'carousel' }) {
  const p = product
  return (
    <Link
      to={`/product/${p.slug}`}
      className={clsx(
        'group flex h-full flex-col bg-white p-3 transition hover:z-10 hover:shadow-[0_2px_16px_rgba(0,0,0,.12)]',
        variant === 'grid' ? 'border-b border-l border-line' : 'rounded-lg',
      )}
    >
      <div className="relative mb-3 aspect-square overflow-hidden rounded-lg">
        <img src={img(p.imageUrl)} alt={p.title} loading="lazy"
          className={clsx('size-full object-contain transition duration-300 group-hover:scale-105', !p.inStock && 'opacity-50 grayscale')} />
        {p.isAmazingOffer && p.inStock && (
          <span className="absolute right-0 top-0 rounded-md bg-brand-500/90 px-2 py-0.5 text-[10px] font-bold text-white">شگفت‌انگیز</span>
        )}
        {p.colors.length > 1 && (
          <div className="absolute bottom-0 left-0 flex flex-col gap-1">
            {p.colors.slice(0, 4).map((c) => (
              <span key={c} className="size-2.5 rounded-full ring-1 ring-black/10" style={{ background: c }} />
            ))}
          </div>
        )}
      </div>
      <h3 className="mb-2 line-clamp-2 min-h-[2.6rem] text-xs leading-5 font-medium text-ink/90">{p.title}</h3>
      <div className="mb-1 flex items-center justify-between">
        <RatingBadge value={p.rating} />
      </div>
      <div className="mt-auto">
        {p.inStock ? (
          <>
            <div className="flex items-center justify-between gap-1">
              <DiscountBadge percent={p.discountPercent} />
              <Price value={p.finalPrice} className="mr-auto" />
            </div>
            {p.discountPercent > 0 && (
              <div className="pl-7 text-left text-xs text-muted line-through">{p.price.toLocaleString('fa-IR')}</div>
            )}
          </>
        ) : (
          <div className="flex items-center gap-2 text-sm font-bold text-muted">
            <span className="h-px flex-1 bg-line" /> ناموجود <span className="h-px flex-1 bg-line" />
          </div>
        )}
      </div>
    </Link>
  )
}

export function ProductCardSkeleton() {
  return (
    <div className="border-b border-l border-line p-3">
      <div className="skeleton mb-3 aspect-square rounded-lg" />
      <div className="skeleton mb-2 h-3 rounded" />
      <div className="skeleton mb-4 h-3 w-2/3 rounded" />
      <div className="skeleton h-4 w-1/2 rounded" />
    </div>
  )
}
