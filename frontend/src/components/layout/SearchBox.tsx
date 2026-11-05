import { useQuery } from '@tanstack/react-query'
import { LayoutGrid, Search } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router'
import { get } from '@/lib/api'
import { img } from '@/lib/format'
import type { Suggestion } from '@/lib/types'

function useDebounced<T>(value: T, delay = 300) {
  const [v, setV] = useState(value)
  useEffect(() => {
    const id = setTimeout(() => setV(value), delay)
    return () => clearTimeout(id)
  }, [value, delay])
  return v
}

export function SearchBox() {
  const [params] = useSearchParams()
  const [term, setTerm] = useState(params.get('q') ?? '')
  const [open, setOpen] = useState(false)
  const debounced = useDebounced(term.trim())
  const navigate = useNavigate()
  const box = useRef<HTMLDivElement>(null)

  const { data } = useQuery({
    queryKey: ['suggest', debounced],
    queryFn: () => get<Suggestion>('/products/suggest', { q: debounced }),
    enabled: debounced.length >= 2,
    staleTime: 60_000,
  })

  useEffect(() => {
    const onClick = (e: MouseEvent) => !box.current?.contains(e.target as Node) && setOpen(false)
    document.addEventListener('mousedown', onClick)
    return () => document.removeEventListener('mousedown', onClick)
  }, [])

  const submit = (e: React.FormEvent) => {
    e.preventDefault()
    setOpen(false)
    if (term.trim()) navigate(`/search?q=${encodeURIComponent(term.trim())}`)
  }

  const hasResults = !!data && (data.products.length > 0 || data.categories.length > 0)

  return (
    <div ref={box} className="relative w-full max-w-xl">
      <form onSubmit={submit} className={`flex items-center gap-2 rounded-lg bg-surface px-3 ${open && hasResults ? 'rounded-b-none' : ''}`}>
        <Search size={20} className="text-muted" />
        <input
          value={term}
          onChange={(e) => { setTerm(e.target.value); setOpen(true) }}
          onFocus={() => setOpen(true)}
          placeholder="جستجو در بازارچه"
          className="h-11 w-full bg-transparent text-sm outline-none placeholder:text-muted"
        />
      </form>
      {open && hasResults && (
        <div className="absolute inset-x-0 top-full z-40 rounded-b-lg border border-t-0 border-line bg-white py-2 shadow-lg">
          {data!.categories.map((c) => (
            <Link key={c.id} to={`/category/${c.slug}`} onClick={() => setOpen(false)}
              className="flex items-center gap-2 px-4 py-2 text-sm hover:bg-surface">
              <LayoutGrid size={16} className="text-muted" /> در دسته <b>{c.name}</b>
            </Link>
          ))}
          {data!.products.map((p) => (
            <Link key={p.slug} to={`/product/${p.slug}`} onClick={() => setOpen(false)}
              className="flex items-center gap-3 px-4 py-2 text-sm hover:bg-surface">
              <img src={img(p.imageUrl)} alt="" className="size-10 rounded object-contain" />
              <span className="line-clamp-1">{p.title}</span>
            </Link>
          ))}
        </div>
      )}
    </div>
  )
}
