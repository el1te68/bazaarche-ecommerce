import { Minus, Plus, ShoppingCart, Trash2 } from 'lucide-react'
import { Link, useNavigate } from 'react-router'
import { formatNumber, img, toFaDigits } from '@/lib/format'
import { useCart } from '@/hooks/useCart'
import { useAuthStore } from '@/stores/auth'
import { EmptyState, PageLoader, Price } from '@/components/ui'

export default function CartPage() {
  const { cart, isLoading, update, remove, isFetching } = useCart()
  const user = useAuthStore((s) => s.user)
  const navigate = useNavigate()

  if (isLoading) return <PageLoader />
  if (cart.items.length === 0)
    return (
      <div className="container-x py-6">
        <div className="card">
          <EmptyState icon={<ShoppingCart size={90} strokeWidth={1} />} title="سبد خرید شما خالی است!"
            description="می‌توانید برای مشاهده محصولات بیشتر به صفحات زیر بروید:"
            action={<div className="flex gap-3"><Link to="/search?amazing=true" className="btn-outline">شگفت‌انگیزها</Link><Link to="/search?sort=bestselling" className="btn-outline">پرفروش‌ترین‌ها</Link></div>} />
        </div>
      </div>
    )

  const unavailable = cart.items.some((i) => i.stock === 0 || i.quantity > i.stock)

  return (
    <div className="container-x grid gap-4 py-6 lg:grid-cols-[1fr_340px]">
      <section className="card p-4 md:p-6">
        <div className="mb-4 flex items-center justify-between">
          <h1 className="text-lg font-bold">سبد خرید شما</h1>
          <span className="text-sm text-muted">{toFaDigits(cart.totalQuantity)} کالا</span>
        </div>
        <ul className={`divide-y divide-line ${isFetching ? 'opacity-70' : ''}`}>
          {cart.items.map((item) => (
            <li key={item.variantId} className="flex gap-4 py-5">
              <Link to={`/product/${item.slug}`} className="shrink-0">
                <img src={img(item.imageUrl)} alt={item.title} className="size-24 rounded-lg object-contain md:size-32" />
              </Link>
              <div className="flex min-w-0 flex-1 flex-col gap-2">
                <Link to={`/product/${item.slug}`} className="line-clamp-2 text-sm font-bold leading-6">{item.title}</Link>
                {item.colorName && (
                  <span className="flex items-center gap-2 text-xs text-ink/80">
                    <span className="size-4 rounded-full ring-1 ring-black/10" style={{ background: item.colorHex ?? '#fff' }} /> {item.colorName}
                  </span>
                )}
                {item.stock === 0 ? (
                  <span className="text-xs font-bold text-brand-600">این کالا ناموجود شده است</span>
                ) : item.quantity > item.stock ? (
                  <span className="text-xs font-bold text-brand-600">فقط {toFaDigits(item.stock)} عدد موجود است</span>
                ) : null}
                <div className="mt-auto flex flex-wrap items-center justify-between gap-3">
                  <div className="flex items-center gap-4 rounded-lg border border-line px-3 py-2">
                    <button onClick={() => update(item.variantId, item.quantity + 1)} disabled={item.quantity >= item.maxQuantity} className="text-brand-500 disabled:text-gray-300"><Plus size={16} /></button>
                    <span className="w-4 text-center font-bold text-brand-500">{toFaDigits(item.quantity)}</span>
                    <button onClick={() => (item.quantity === 1 ? remove(item.variantId) : update(item.variantId, item.quantity - 1))} className="text-brand-500">
                      {item.quantity === 1 ? <Trash2 size={16} /> : <Minus size={16} />}
                    </button>
                  </div>
                  <div className="text-left">
                    {item.finalPrice < item.price && (
                      <div className="text-xs text-brand-500">{formatNumber((item.price - item.finalPrice) * item.quantity)} تومان تخفیف</div>
                    )}
                    <Price value={item.finalPrice * item.quantity} />
                  </div>
                </div>
              </div>
            </li>
          ))}
        </ul>
      </section>

      <aside className="h-fit space-y-4 lg:sticky lg:top-36">
        <div className="card space-y-4 p-5 text-sm">
          <div className="flex justify-between text-muted"><span>قیمت کالاها ({toFaDigits(cart.totalQuantity)})</span><span>{formatNumber(cart.itemsTotal)} تومان</span></div>
          <div className="flex justify-between font-bold"><span>جمع سبد خرید</span><Price value={cart.payableTotal} /></div>
          {cart.productsDiscount > 0 && (
            <div className="flex justify-between font-bold text-brand-500"><span>سود شما از خرید</span><span>{formatNumber(cart.productsDiscount)} تومان</span></div>
          )}
          <button className="btn-primary w-full py-3" disabled={unavailable}
            onClick={() => navigate(user ? '/checkout' : '/login?returnUrl=/checkout')}>
            {user ? 'تایید و تکمیل سفارش' : 'ورود و ادامه خرید'}
          </button>
          {unavailable && <p className="text-xs text-brand-600">برای ادامه، کالاهای ناموجود را حذف یا تعدادشان را اصلاح کنید.</p>}
        </div>
        <p className="px-2 text-xs leading-6 text-muted">
          کالاهای موجود در سبد شما ثبت و رزرو نشده‌اند، برای ثبت سفارش مراحل بعدی را تکمیل کنید.
        </p>
      </aside>
    </div>
  )
}
