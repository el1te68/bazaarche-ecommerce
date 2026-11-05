import { ChevronLeft } from 'lucide-react'
import type { ReactNode } from 'react'
import { Link } from 'react-router'
import { Navigation } from 'swiper/modules'
import { Swiper, SwiperSlide } from 'swiper/react'
import 'swiper/css'
import 'swiper/css/navigation'
import type { ProductCard as Product } from '@/lib/types'
import { ProductCard } from './ProductCard'

export function ProductCarousel({ title, products, moreLink, icon }: { title: string; products: Product[]; moreLink?: string; icon?: ReactNode }) {
  if (products.length === 0) return null
  return (
    <section className="card overflow-hidden py-4">
      <div className="mb-3 flex items-center justify-between px-4">
        <h2 className="flex items-center gap-2 text-lg font-bold">{icon}{title}</h2>
        {moreLink && (
          <Link to={moreLink} className="flex items-center gap-1 text-sm font-medium text-sky-600">
            مشاهده همه <ChevronLeft size={16} />
          </Link>
        )}
      </div>
      <Swiper modules={[Navigation]} navigation slidesPerView={2.2} spaceBetween={0}
        breakpoints={{ 640: { slidesPerView: 3.2 }, 900: { slidesPerView: 4.3 }, 1200: { slidesPerView: 6 } }}>
        {products.map((p) => (
          <SwiperSlide key={p.id} className="!h-auto border-l border-line last:border-l-0">
            <ProductCard product={p} variant="carousel" />
          </SwiperSlide>
        ))}
      </Swiper>
    </section>
  )
}
