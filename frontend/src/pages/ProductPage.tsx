import { useQuery } from '@tanstack/react-query'
import { clsx } from 'clsx'
import { Check, Heart, Minus, Plus, Share2, ShieldCheck, Store, Trash2, Truck } from 'lucide-react'
import { useEffect, useMemo, useState } from 'react'
import toast from 'react-hot-toast'
import { Link, useParams } from 'react-router'
import { get } from '@/lib/api'
import { formatNumber, img, toFaDigits } from '@/lib/format'
import type { ProductCard, ProductDetail } from '@/lib/types'
import { useCart } from '@/hooks/useCart'
import { useWishlist } from '@/hooks/useWishlist'
import { Breadcrumbs, Countdown, DiscountBadge, PageLoader, Price, RatingBadge, Spinner } from '@/components/ui'
import { ProductCarousel } from '@/components/product/ProductCarousel'
import { Reviews } from '@/components/product/Reviews'

function Gallery({ images, title }: { images: string[]; title: string }) {
  const [active, setActive] = useState(0)
  useEffect(() => setActive(0), [images])
  return (
    <div className="space-y-3">
      <div className="aspect-square overflow-hidden rounded-xl">
        <img src={img(images[active])} alt={title} className="size-full object-contain" />
      </div>
      {images.length > 1 && (
        <div className="flex gap-2">
          {images.map((src, i) => (
            <button key={src} onClick={() => setActive(i)}
              className={clsx('size-16 overflow-hidden rounded-lg border-2 p-1', i === active ? 'border-brand-500' : 'border-line')}>
              <img src={img(src)} alt="" className="size-full object-contain" />
            </button>
          ))}
        </div>
      )}
    </div>
  )
}

export default function ProductPage() {
  const { slug = '' } = useParams()
  const { data: p, isLoading, isError } = useQuery({ queryKey: ['product', slug], queryFn: () => get<ProductDetail>(`/products/${encodeURIComponent(slug)}`) })
  const { data: related = [] } = useQuery({
    queryKey: ['related', p?.id], queryFn: () => get<ProductCard[]>(`/products/${p!.id}/related`), enabled: !!p,
  })
  const { quantityOf, add, update, adding, isFetching } = useCart()
  const wishlist = useWishlist()
  const [variantId, setVariantId] = useState<number>()
  const [tab, setTab] = useState<'intro' | 'specs' | 'reviews'>('intro')

  const variant = useMemo(() => {
    if (!p) return undefined
    return p.variants.find((v) => v.id === variantId) ?? p.variants.find((v) => v.stock > 0) ?? p.variants[0]
  }, [p, variantId])

  if (isLoading) return <PageLoader />
  if (isError || !p) return <div className="container-x py-20 text-center text-muted">محصول موردنظر پیدا نشد.</div>

  const inCart = variant ? quantityOf(variant.id) : 0
  const maxQty = variant ? Math.min(variant.stock, 10) : 0
  const hasColors = p.variants.some((v) => v.colorHex)

  const addToCart = async () => {
    if (!variant) return
    await add(variant.id, 1, maxQty)
    toast.success('به سبد خرید اضافه شد')
  }

  const share = async () => {
    try {
      await navigator.clipboard.writeText(window.location.href)
      toast.success('لینک محصول کپی شد')
    } catch { /* clipboard not available */ }
  }

  return (
    <div className="container-x py-4">
      <div className="mb-4"><Breadcrumbs items={p.breadcrumbs} /></div>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,420px)_1fr_330px]">
        <div className="relative">
          {p.isAmazingOffer && p.offerEndsAt && (
            <div className="mb-3 flex items-center justify-between rounded-xl bg-brand-500 px-4 py-2 text-white">
              <span className="font-bold">شگفت‌انگیز</span>
              <Countdown to={p.offerEndsAt} />
            </div>
          )}
          <div className="relative">
            <div className="absolute left-0 top-0 z-10 flex flex-col gap-2">
              <button onClick={() => wishlist.toggle(p.id)} className="rounded-full bg-white p-2 shadow-card" aria-label="علاقه‌مندی">
              <Heart size={22} className={wishlist.has(p.id) || p.isInWishlist ? 'fill-brand-500 text-brand-500' : ''} />
            </button>
              <button onClick={share} className="rounded-full bg-white p-2 shadow-card" aria-label="اشتراک‌گذاری"><Share2 size={22} /></button>
            </div>
            <Gallery images={p.images} title={p.title} />
          </div>
        </div>

        <div className="space-y-4">
          {p.brand && (
            <Link to={`/search?brands=${p.brand.slug}`} className="text-sm font-medium text-sky-600">{p.brand.name}</Link>
          )}
          <h1 className="text-lg font-bold leading-8">{p.title}</h1>
          {p.englishTitle && <p className="text-xs text-muted" dir="ltr">{p.englishTitle}</p>}
          <div className="flex flex-wrap items-center gap-3 text-xs">
            <RatingBadge value={p.rating} count={p.reviewCount} />
            <button onClick={() => setTab('reviews')} className="text-sky-600">{toFaDigits(p.reviewCount)} دیدگاه</button>
            {p.soldCount > 0 && <span className="rounded-full bg-surface px-2 py-1 text-ink/70">{formatNumber(p.soldCount)} فروش موفق</span>}
          </div>

          {hasColors && variant && (
            <div className="border-t border-line pt-4">
              <h3 className="mb-3 font-bold">رنگ: <span className="font-normal">{variant.colorName}</span></h3>
              <div className="flex flex-wrap gap-2">
                {p.variants.map((v) => (
                  <button key={v.id} onClick={() => setVariantId(v.id)} title={v.colorName ?? ''}
                    className={clsx('relative flex items-center gap-2 rounded-full border-2 p-1 pl-3 text-xs', v.id === variant.id ? 'border-sky-500' : 'border-line', v.stock === 0 && 'opacity-50')}>
                    <span className="flex size-7 items-center justify-center rounded-full ring-1 ring-black/10" style={{ background: v.colorHex ?? '#fff' }}>
                      {v.id === variant.id && <Check size={14} className="text-white mix-blend-difference" />}
                    </span>
                    {v.colorName}
                  </button>
                ))}
              </div>
            </div>
          )}

          {p.specifications.length > 0 && (
            <div className="border-t border-line pt-4">
              <h3 className="mb-3 font-bold">ویژگی‌ها</h3>
              <div className="grid grid-cols-2 gap-2 xl:grid-cols-3">
                {p.specifications.slice(0, 6).map((s) => (
                  <div key={s.key} className="rounded-lg bg-surface p-2.5">
                    <div className="text-xs text-muted">{s.key}</div>
                    <div className="line-clamp-2 text-sm font-bold">{s.value}</div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Buy box */}
        <aside className="h-fit space-y-4 rounded-xl border border-line bg-surface/60 p-5 lg:sticky lg:top-36">
          <h3 className="font-bold">فروشنده</h3>
          <div className="flex items-center gap-3 border-b border-line pb-4 text-sm">
            <Store className="text-brand-500" /> <div><b>بازارچه</b><div className="text-xs text-muted">عملکرد عالی</div></div>
          </div>
          <div className="flex items-center gap-3 border-b border-line pb-4 text-sm"><ShieldCheck className="text-ink/70" /> گارانتی اصالت و سلامت فیزیکی کالا</div>
          <div className="flex items-center gap-3 border-b border-line pb-4 text-sm"><Truck className="text-ink/70" /> ارسال توسط بازارچه</div>

          {variant && variant.stock > 0 ? (
            <>
              <div className="space-y-1 text-left">
                {variant.discountPercent > 0 && (
                  <div className="flex items-center justify-end gap-2">
                    <span className="text-sm text-muted line-through">{formatNumber(variant.price)}</span>
                    <DiscountBadge percent={variant.discountPercent} />
                  </div>
                )}
                <Price value={variant.finalPrice} size="lg" />
              </div>
              {variant.stock <= 5 && <p className="text-xs font-bold text-brand-600">تنها {toFaDigits(variant.stock)} عدد در انبار باقی مانده</p>}
              {inCart > 0 ? (
                <div className="flex items-center gap-3">
                  <div className="flex items-center gap-4 rounded-lg border border-line bg-white px-3 py-2 shadow-card">
                    <button onClick={() => update(variant.id, inCart + 1)} disabled={inCart >= maxQty || isFetching} className="text-brand-500 disabled:text-gray-300"><Plus size={18} /></button>
                    <span className="w-4 text-center font-bold text-brand-500">{isFetching ? <Spinner className="size-4" /> : toFaDigits(inCart)}</span>
                    <button onClick={() => update(variant.id, inCart - 1)} className="text-brand-500">{inCart === 1 ? <Trash2 size={18} /> : <Minus size={18} />}</button>
                  </div>
                  <div className="text-xs">در سبد شما<br /><Link to="/cart" className="font-bold text-sky-600">مشاهده سبد خرید</Link></div>
                </div>
              ) : (
                <button onClick={addToCart} disabled={adding} className="btn-primary w-full py-3">
                  {adding ? <Spinner className="size-5 text-white" /> : 'افزودن به سبد خرید'}
                </button>
              )}
            </>
          ) : (
            <div className="rounded-lg bg-white p-4 text-center">
              <p className="mb-1 font-bold text-muted">ناموجود</p>
              <p className="text-xs text-muted">این کالا فعلا موجود نیست اما می‌توانید زنگوله را بزنید تا به محض موجود شدن، به شما خبر دهیم.</p>
            </div>
          )}
        </aside>
      </div>

      {/* Tabs */}
      <div className="mt-10">
        <div className="sticky top-[108px] z-20 flex gap-6 border-b border-line bg-white text-sm md:top-[118px]">
          {([['intro', 'معرفی'], ['specs', 'مشخصات'], ['reviews', 'دیدگاه‌ها']] as const).map(([key, label]) => (
            <button key={key} onClick={() => setTab(key)}
              className={clsx('border-b-4 px-1 py-3', tab === key ? 'border-brand-500 font-bold text-brand-500' : 'border-transparent text-muted')}>
              {label}
            </button>
          ))}
        </div>
        <div className="py-6">
          {tab === 'intro' && <p className="max-w-4xl whitespace-pre-line text-sm leading-8 text-ink/85">{p.description}</p>}
          {tab === 'specs' && (
            <dl className="max-w-4xl divide-y divide-line">
              {p.specifications.map((s) => (
                <div key={s.key} className="grid grid-cols-[140px_1fr] gap-4 py-3 text-sm sm:grid-cols-[220px_1fr]">
                  <dt className="text-muted">{s.key}</dt><dd className="font-medium">{s.value}</dd>
                </div>
              ))}
            </dl>
          )}
          {tab === 'reviews' && <Reviews productId={p.id} />}
        </div>
      </div>

      <div className="mt-4"><ProductCarousel title="کالاهای مشابه" products={related} /></div>
    </div>
  )
}
