import { ChevronLeft, SearchX } from 'lucide-react'
import { Link } from 'react-router'
import { useCategories } from '@/components/layout/Header'
import { CategoryIcon } from '@/components/ui/CategoryIcon'
import { EmptyState, PageLoader } from '@/components/ui'

export function CategoriesPage() {
  const { data = [], isLoading } = useCategories()
  if (isLoading) return <PageLoader />
  return (
    <div className="container-x space-y-4 py-4">
      {data.map((c) => (
        <section key={c.id} className="card p-4">
          <Link to={`/category/${c.slug}`} className="mb-3 flex items-center gap-2 font-bold">
            <CategoryIcon name={c.icon} className="text-brand-500" /> {c.name} <ChevronLeft size={16} className="mr-auto text-muted" />
          </Link>
          <div className="flex flex-wrap gap-2">
            {c.children.map((ch) => <Link key={ch.id} to={`/category/${ch.slug}`} className="chip py-2">{ch.name}</Link>)}
          </div>
        </section>
      ))}
    </div>
  )
}

export function NotFoundPage() {
  return (
    <div className="container-x py-10">
      <EmptyState icon={<SearchX size={90} strokeWidth={1} />} title="صفحه‌ای که دنبال آن بودید پیدا نشد!"
        action={<Link to="/" className="btn-primary">صفحه اصلی بازارچه</Link>} />
    </div>
  )
}
