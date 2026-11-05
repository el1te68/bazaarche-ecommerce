import { keepPreviousData, useQuery } from '@tanstack/react-query'
import { clsx } from 'clsx'
import { ArrowDownWideNarrow, Filter, PackageSearch, X } from 'lucide-react'
import { useState } from 'react'
import { Link, useParams, useSearchParams } from 'react-router'
import { get } from '@/lib/api'
import { formatNumber, toEnDigits, toFaDigits } from '@/lib/format'
import type { ProductListResponse } from '@/lib/types'
import { Breadcrumbs, EmptyState, Modal, Pagination, Toggle } from '@/components/ui'
import { ProductCard, ProductCardSkeleton } from '@/components/product/ProductCard'

const sorts = [
  { key: 'newest', label: 'جدیدترین' },
  { key: 'bestselling', label: 'پرفروش‌ترین' },
  { key: 'popular', label: 'پربازدیدترین' },
  { key: 'cheapest', label: 'ارزان‌ترین' },
  { key: 'expensive', label: 'گران‌ترین' },
  { key: 'rating', label: 'پیشنهاد خریداران' },
  { key: 'discount', label: 'بیشترین تخفیف' },
]

function Filters({ data, params, update }: {
  data?: ProductListResponse
  params: URLSearchParams
  update: (changes: Record<string, string | null>) => void
}) {
  const selectedBrands = (params.get('brands') ?? '').split(',').filter(Boolean)
  const [min, setMin] = useState(params.get('minPrice') ?? '')
  const [max, setMax] = useState(params.get('maxPrice') ?? '')

  const toggleBrand = (slug: string) => {
    const next = selectedBrands.includes(slug) ? selectedBrands.filter((s) => s !== slug) : [...selectedBrands, slug]
    update({ brands: next.length ? next.join(',') : null })
  }

  const hasFilters = ['brands', 'minPrice', 'maxPrice', 'inStock', 'discount', 'amazing'].some((k) => params.get(k))

  return (
    <div className="space-y-1">
      <div className="flex items-center justify-between pb-3">
        <h3 className="font-bold">فیلترها</h3>
        {hasFilters && (
          <button className="text-xs text-sky-600"
            onClick={() => update({ brands: null, minPrice: null, maxPrice: null, inStock: null, discount: null, amazing: null })}>
            حذف فیلترها
          </button>
        )}
      </div>

      {data?.category && data.category.children.length > 0 && (
        <details open className="border-b border-line py-3">
          <summary className="cursor-pointer text-sm font-bold">دسته‌بندی</summary>
          <ul className="mt-3 space-y-2 pr-2 text-sm">
            {data.category.children.map((c) => (
              <li key={c.id}><Link to={`/category/${c.slug}`} className="text-ink/80 hover:text-brand-500">{c.name}</Link></li>
            ))}
          </ul>
        </details>
      )}

      {!!data?.brands.length && (
        <details open className="border-b border-line py-3">
          <summary className="cursor-pointer text-sm font-bold">برند</summary>
          <ul className="mt-3 max-h-60 space-y-2 overflow-y-auto pr-1">
            {data.brands.map((b) => (
              <li key={b.id}>
                <label className="flex cursor-pointer items-center gap-2 text-sm">
                  <input type="checkbox" className="size-4 accent-brand-500" checked={selectedBrands.includes(b.slug)} onChange={() => toggleBrand(b.slug)} />
                  <span className="flex-1">{b.name}</span>
                  <span className="text-xs text-muted">{toFaDigits(b.count)}</span>
                </label>
              </li>
            ))}
          </ul>
        </details>
      )}

      <details open className="border-b border-line py-3">
        <summary className="cursor-pointer text-sm font-bold">محدوده قیمت (تومان)</summary>
        <form className="mt-3 space-y-2" onSubmit={(e) => { e.preventDefault(); update({ minPrice: toEnDigits(min) || null, maxPrice: toEnDigits(max) || null }) }}>
          <div className="flex items-center gap-2">
            <span className="w-6 text-xs text-muted">از</span>
            <input className="input text-left" dir="ltr" inputMode="numeric" value={min} onChange={(e) => setMin(e.target.value.replace(/[^\d۰-۹]/g, ''))} placeholder="0" />
          </div>
          <div className="flex items-center gap-2">
            <span className="w-6 text-xs text-muted">تا</span>
            <input className="input text-left" dir="ltr" inputMode="numeric" value={max} onChange={(e) => setMax(e.target.value.replace(/[^\d۰-۹]/g, ''))}
              placeholder={data?.maxPrice ? String(data.maxPrice) : ''} />
          </div>
          <button className="btn-outline w-full py-2">اعمال قیمت</button>
        </form>
      </details>

      <div className="space-y-4 py-4">
        <Toggle label="فقط کالاهای موجود" checked={params.get('inStock') === 'true'} onChange={(v) => update({ inStock: v ? 'true' : null })} />
        <Toggle label="فقط کالاهای تخفیف‌دار" checked={params.get('discount') === 'true'} onChange={(v) => update({ discount: v ? 'true' : null })} />
        <Toggle label="فقط شگفت‌انگیزها" checked={params.get('amazing') === 'true'} onChange={(v) => update({ amazing: v ? 'true' : null })} />
      </div>
    </div>
  )
}

export default function ProductListPage() {
  const { slug } = useParams()
  const [params, setParams] = useSearchParams()
  const [filtersOpen, setFiltersOpen] = useState(false)
  const page = Number(params.get('page') ?? 1)
  const sort = params.get('sort') ?? 'newest'
  const q = params.get('q') ?? ''

  const query = {
    category: slug, q: q || undefined, brands: params.get('brands') || undefined,
    minPrice: params.get('minPrice') || undefined, maxPrice: params.get('maxPrice') || undefined,
    inStock: params.get('inStock') === 'true' || undefined, hasDiscount: params.get('discount') === 'true' || undefined,
    amazingOnly: params.get('amazing') === 'true' || undefined, sort, page, pageSize: 24,
  }

  const { data, isLoading, isFetching, isError } = useQuery({
    queryKey: ['products', query],
    queryFn: () => get<ProductListResponse>('/products', query),
    placeholderData: keepPreviousData,
  })

  const update = (changes: Record<string, string | null>) => {
    const next = new URLSearchParams(params)
    Object.entries(changes).forEach(([k, v]) => (v === null || v === '' ? next.delete(k) : next.set(k, v)))
    if (!('page' in changes)) next.delete('page')
    setParams(next)
    setFiltersOpen(false)
  }

  const title = data?.category?.name ?? (q ? `نتایج جستجو برای «${q}»` : params.get('amazing') ? 'پیشنهادهای شگفت‌انگیز' : 'همه محصولات')

  if (isError) return <div className="container-x py-20 text-center text-muted">دسته‌بندی یا صفحه موردنظر پیدا نشد.</div>

  return (
    <div className="container-x py-4">
      <div className="mb-4 space-y-2">
        {data?.breadcrumbs && data.breadcrumbs.length > 0 && <Breadcrumbs items={data.breadcrumbs} />}
        <h1 className="text-xl font-bold">{title}</h1>
        {data?.category && data.category.children.length > 0 && (
          <div className="scrollbar-none flex gap-2 overflow-x-auto pt-2">
            {data.category.children.map((c) => (
              <Link key={c.id} to={`/category/${c.slug}`} className="chip shrink-0 bg-white py-2 hover:border-brand-400">{c.name}</Link>
            ))}
          </div>
        )}
      </div>

      <div className="flex gap-4">
        <aside className="card sticky top-36 hidden h-fit w-72 shrink-0 p-4 lg:block">
          <Filters key={params.toString()} data={data} params={params} update={update} />
        </aside>

        <div className="min-w-0 flex-1">
          <div className="mb-3 flex items-center gap-3 border-b border-line pb-3">
            <button onClick={() => setFiltersOpen(true)} className="chip py-2 lg:hidden"><Filter size={14} /> فیلترها</button>
            <ArrowDownWideNarrow size={20} className="hidden text-ink sm:block" />
            <span className="hidden text-sm font-bold sm:block">مرتب‌سازی:</span>
            <div className="scrollbar-none flex flex-1 gap-4 overflow-x-auto text-sm">
              {sorts.map((s) => (
                <button key={s.key} onClick={() => update({ sort: s.key })}
                  className={clsx('shrink-0 py-1', sort === s.key ? 'font-bold text-brand-500' : 'text-muted hover:text-ink')}>
                  {s.label}
                </button>
              ))}
            </div>
            {data && <span className="shrink-0 text-xs text-muted">{formatNumber(data.products.totalCount)} کالا</span>}
          </div>

          {params.get('brands') && (
            <div className="mb-3 flex flex-wrap gap-2">
              {params.get('brands')!.split(',').map((b) => (
                <button key={b} className="chip bg-brand-50 text-brand-600"
                  onClick={() => update({ brands: params.get('brands')!.split(',').filter((x) => x !== b).join(',') || null })}>
                  {data?.brands.find((x) => x.slug === b)?.name ?? b} <X size={12} />
                </button>
              ))}
            </div>
          )}

          <div className={clsx('grid grid-cols-2 border-t border-r border-line sm:grid-cols-3 xl:grid-cols-4', isFetching && !isLoading && 'opacity-60')}>
            {isLoading && Array.from({ length: 12 }).map((_, i) => <ProductCardSkeleton key={i} />)}
            {data?.products.items.map((p) => <ProductCard key={p.id} product={p} />)}
          </div>

          {data && data.products.items.length === 0 && (
            <EmptyState icon={<PackageSearch size={72} strokeWidth={1} />} title="کالایی با این مشخصات پیدا نکردیم"
              description="پیشنهاد می‌کنیم فیلترها را تغییر دهید یا عبارت دیگری را جستجو کنید." />
          )}

          {data && <Pagination page={page} totalPages={data.products.totalPages} onChange={(p) => { update({ page: String(p) }); window.scrollTo({ top: 0 }) }} />}
        </div>
      </div>

      <Modal open={filtersOpen} onClose={() => setFiltersOpen(false)} title="فیلترها">
        <Filters key={params.toString()} data={data} params={params} update={update} />
      </Modal>
    </div>
  )
}
