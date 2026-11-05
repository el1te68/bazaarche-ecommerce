import { useQuery } from '@tanstack/react-query'
import { ChevronLeft, Crown, Flame, Percent, Sparkles, TrendingUp } from 'lucide-react'
import { Link } from 'react-router'
import { Autoplay, Navigation, Pagination } from 'swiper/modules'
import { Swiper, SwiperSlide } from 'swiper/react'
import 'swiper/css'
import 'swiper/css/pagination'
import 'swiper/css/navigation'
import { get } from '@/lib/api'
import { img } from '@/lib/format'
import type { HomeData, ProductCard as Product } from '@/lib/types'
import { CategoryIcon } from '@/components/ui/CategoryIcon'
import { Countdown, DiscountBadge, Price, Skeleton } from '@/components/ui'
import { ProductCarousel } from '@/components/product/ProductCarousel'

function AmazingOffers({ products }: { products: Product[] }) {
  if (products.length === 0) return null
  const endsAt = products.map((p) => p.offerEndsAt).filter(Boolean).sort()[0]
  return (
    <section className="overflow-hidden rounded-2xl bg-gradient-to-l from-brand-500 to-brand-700 py-5">
      <Swiper modules={[Navigation]} navigation slidesPerView={2.3} spaceBetween={4}
        breakpoints={{ 640: { slidesPerView: 3.3 }, 900: { slidesPerView: 4.5 }, 1200: { slidesPerView: 6.3 } }}>
        <SwiperSlide className="!h-auto">
          <div className="flex h-full flex-col items-center justify-center gap-4 px-3 text-center text-white">
            <Percent size={48} strokeWidth={1.5} className="rotate-12 rounded-2xl border-2 border-white/70 p-2" />
            <h2 className="text-2xl font-black leading-snug">پیشنهاد<br />شگفت‌انگیز</h2>
            {endsAt && <Countdown to={endsAt} />}
            <Link to="/search?amazing=true" className="flex items-center gap-1 text-sm">مشاهده همه <ChevronLeft size={16} /></Link>
          </div>
        </SwiperSlide>
        {products.map((p, i) => (
          <SwiperSlide key={p.id} className="!h-auto">
            <Link to={`/product/${p.slug}`}
              className={`flex h-full flex-col bg-white p-3 ${i === 0 ? 'rounded-r-lg' : ''} ${i === products.length - 1 ? 'rounded-l-lg' : ''}`}>
              <img src={img(p.imageUrl)} alt={p.title} className="mb-3 aspect-square w-full object-contain" loading="lazy" />
              <h3 className="mb-3 line-clamp-2 text-xs leading-5 text-ink/90">{p.title}</h3>
              <div className="mt-auto flex items-center justify-between">
                <DiscountBadge percent={p.discountPercent} />
                <Price value={p.finalPrice} size="sm" />
              </div>
              <div className="text-left text-xs text-muted line-through">{p.price.toLocaleString('fa-IR')}</div>
            </Link>
          </SwiperSlide>
        ))}
        <SwiperSlide className="!h-auto">
          <Link to="/search?amazing=true" className="mr-1 flex h-full flex-col items-center justify-center gap-3 rounded-l-lg bg-white p-4 text-sm font-bold text-ink">
            <span className="flex size-12 items-center justify-center rounded-full border border-sky-500 text-sky-500"><ChevronLeft /></span>
            مشاهده همه
          </Link>
        </SwiperSlide>
      </Swiper>
    </section>
  )
}

function HomeSkeleton() {
  return (
    <div className="container-x space-y-6 py-4">
      <Skeleton className="aspect-[10/3] w-full rounded-2xl" />
      <div className="flex gap-6 overflow-hidden">{Array.from({ length: 8 }).map((_, i) => <Skeleton key={i} className="size-24 shrink-0 rounded-full" />)}</div>
      <Skeleton className="h-80 w-full rounded-2xl" />
    </div>
  )
}

export default function HomePage() {
  const { data, isLoading, isError } = useQuery({ queryKey: ['home'], queryFn: () => get<HomeData>('/home') })
  if (isLoading) return <HomeSkeleton />
  if (isError || !data) return <div className="container-x py-20 text-center text-muted">بارگذاری صفحه با خطا مواجه شد.</div>

  return (
    <div className="space-y-6 pb-6 md:space-y-8">
      {data.heroBanners.length > 0 && (
        <Swiper modules={[Autoplay, Pagination, Navigation]} autoplay={{ delay: 5000, disableOnInteraction: false }}
          pagination={{ clickable: true }} navigation loop className="aspect-[10/3] max-h-[420px] w-full bg-surface">
          {data.heroBanners.map((b) => (
            <SwiperSlide key={b.id}>
              <Link to={b.linkUrl || '/'}><img src={img(b.imageUrl)} alt={b.title} className="size-full object-cover" /></Link>
            </SwiperSlide>
          ))}
        </Swiper>
      )}

      <div className="container-x space-y-6 md:space-y-8">
        {/* Categories */}
        <section>
          <h2 className="mb-5 text-center text-lg font-bold">خرید بر اساس دسته‌بندی</h2>
          <div className="scrollbar-none flex gap-4 overflow-x-auto pb-2 sm:grid sm:grid-cols-5 lg:grid-cols-9 sm:overflow-visible">
            {data.categories.map((c) => (
              <Link key={c.id} to={`/category/${c.slug}`} className="group flex w-20 shrink-0 flex-col items-center gap-2 sm:w-auto">
                <span className="flex size-20 items-center justify-center rounded-full bg-surface text-ink/80 transition group-hover:bg-brand-50 group-hover:text-brand-500 md:size-24">
                  <CategoryIcon name={c.icon} size={36} />
                </span>
                <span className="text-center text-xs font-medium">{c.name}</span>
              </Link>
            ))}
          </div>
        </section>

        <AmazingOffers products={data.amazingOffers} />

        {data.middleBanners.length > 0 && (
          <section className="grid grid-cols-2 gap-3 lg:grid-cols-4">
            {data.middleBanners.map((b) => (
              <Link key={b.id} to={b.linkUrl || '/'} className="overflow-hidden rounded-2xl">
                <img src={img(b.imageUrl)} alt={b.title} className="aspect-[2/1] w-full object-cover transition hover:scale-105" loading="lazy" />
              </Link>
            ))}
          </section>
        )}

        <ProductCarousel title="پرفروش‌ترین کالاها" icon={<TrendingUp className="text-brand-500" />} products={data.bestsellers} moreLink="/search?sort=bestselling" />
        <ProductCarousel title="بیشترین تخفیف‌ها" icon={<Flame className="text-orange-500" />} products={data.topDiscounts} moreLink="/search?discount=true&sort=discount" />
        <ProductCarousel title="جدیدترین محصولات" icon={<Sparkles className="text-amber-500" />} products={data.newest} moreLink="/search?sort=newest" />
        <ProductCarousel title="محبوب‌ترین‌ها از نگاه خریداران" icon={<Crown className="text-yellow-500" />} products={data.topRated} moreLink="/search?sort=rating" />

        {data.brands.length > 0 && (
          <section className="card p-4">
            <h2 className="mb-4 text-center text-lg font-bold">محبوب‌ترین برندها</h2>
            <div className="scrollbar-none flex gap-0 overflow-x-auto">
              {data.brands.map((b) => (
                <Link key={b.id} to={`/search?brands=${b.slug}`} title={b.name}
                  className="flex h-24 w-36 shrink-0 items-center justify-center border-l border-line px-4 last:border-l-0">
                  <img src={img(b.logoUrl)} alt={b.name} className="max-h-16 w-full object-contain opacity-80 transition hover:opacity-100" loading="lazy" />
                </Link>
              ))}
            </div>
          </section>
        )}
      </div>
    </div>
  )
}
