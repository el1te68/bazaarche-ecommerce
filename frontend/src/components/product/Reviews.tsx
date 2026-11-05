import { useMutation, useQuery } from '@tanstack/react-query'
import { clsx } from 'clsx'
import { BadgeCheck, MessageSquareText, Star } from 'lucide-react'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { Link } from 'react-router'
import { errorMessage, get, post } from '@/lib/api'
import { formatDate, toFaDigits } from '@/lib/format'
import type { ReviewSummary } from '@/lib/types'
import { useAuthStore } from '@/stores/auth'
import { Field, Modal, Pagination, RatingStars } from '@/components/ui'

function ReviewForm({ productId, onDone }: { productId: number; onDone: () => void }) {
  const [rating, setRating] = useState(5)
  const [title, setTitle] = useState('')
  const [comment, setComment] = useState('')
  const mutation = useMutation({
    mutationFn: () => post<{ message: string }>(`/products/${productId}/reviews`, { rating, title, comment }),
    onSuccess: (r) => { toast.success(r.message); onDone() },
    onError: (e) => toast.error(errorMessage(e)),
  })
  return (
    <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
      <div>
        <span className="label">امتیاز شما</span>
        <div className="flex gap-1" dir="ltr">
          {[1, 2, 3, 4, 5].map((i) => (
            <button type="button" key={i} onClick={() => setRating(i)} aria-label={`${i} ستاره`}>
              <Star size={30} className={i <= rating ? 'fill-amber-400 text-amber-400' : 'text-gray-300'} />
            </button>
          ))}
        </div>
      </div>
      <Field label="عنوان نظر"><input className="input" value={title} onChange={(e) => setTitle(e.target.value)} required maxLength={100} /></Field>
      <Field label="متن نظر"><textarea className="input min-h-32" value={comment} onChange={(e) => setComment(e.target.value)} required maxLength={2000} /></Field>
      <button className="btn-primary w-full" disabled={mutation.isPending}>ثبت نظر</button>
    </form>
  )
}

export function Reviews({ productId }: { productId: number }) {
  const [page, setPage] = useState(1)
  const [open, setOpen] = useState(false)
  const user = useAuthStore((s) => s.user)
  const { data } = useQuery({
    queryKey: ['reviews', productId, page],
    queryFn: () => get<ReviewSummary>(`/products/${productId}/reviews`, { page }),
  })

  return (
    <div className="grid gap-8 lg:grid-cols-[280px_1fr]">
      <div className="space-y-4">
        {data && data.count > 0 && (
          <>
            <div className="flex items-end gap-2">
              <span className="text-4xl font-black">{toFaDigits(data.average.toFixed(1))}</span>
              <span className="pb-1 text-sm text-muted">از ۵</span>
            </div>
            <RatingStars value={data.average} size={18} />
            <p className="text-xs text-muted">از مجموع {toFaDigits(data.count)} امتیاز</p>
            <div className="space-y-1.5">
              {[5, 4, 3, 2, 1].map((r) => {
                const n = data.distribution[String(r)] ?? 0
                return (
                  <div key={r} className="flex items-center gap-2 text-xs">
                    <span className="w-3">{toFaDigits(r)}</span>
                    <div className="h-1.5 flex-1 overflow-hidden rounded-full bg-surface">
                      <div className="h-full rounded-full bg-amber-400" style={{ width: `${data.count ? (n / data.count) * 100 : 0}%` }} />
                    </div>
                    <span className="w-5 text-muted">{toFaDigits(n)}</span>
                  </div>
                )
              })}
            </div>
          </>
        )}
        <div className="rounded-xl border border-line p-4">
          <p className="mb-3 text-sm">شما هم درباره این کالا دیدگاه ثبت کنید</p>
          {user ? (
            <button onClick={() => setOpen(true)} className="btn-outline w-full">ثبت دیدگاه</button>
          ) : (
            <Link to="/login" className="btn-outline w-full">برای ثبت نظر وارد شوید</Link>
          )}
        </div>
      </div>

      <div>
        {data && data.reviews.items.length === 0 && (
          <div className="flex flex-col items-center gap-2 py-10 text-muted">
            <MessageSquareText size={48} strokeWidth={1} />
            هنوز دیدگاهی برای این کالا ثبت نشده. اولین نفر باشید!
          </div>
        )}
        <ul className="divide-y divide-line">
          {data?.reviews.items.map((r) => (
            <li key={r.id} className="py-4">
              <div className="mb-2 flex items-center gap-3">
                <span className={clsx('rounded-md px-1.5 py-0.5 text-xs font-bold text-white', r.rating >= 4 ? 'bg-emerald-500' : r.rating >= 3 ? 'bg-amber-500' : 'bg-rose-500')}>
                  {toFaDigits(r.rating)}
                </span>
                <h4 className="font-bold">{r.title}</h4>
              </div>
              <div className="mb-3 flex flex-wrap items-center gap-2 text-xs text-muted">
                <span>{formatDate(r.createdAt)}</span><span>•</span><span>{r.userName}</span>
                {r.isBuyer && <span className="flex items-center gap-1 rounded-full bg-emerald-50 px-2 py-0.5 text-emerald-700"><BadgeCheck size={13} /> خریدار</span>}
              </div>
              <p className="text-sm leading-7 text-ink/85">{r.comment}</p>
            </li>
          ))}
        </ul>
        {data && <Pagination page={page} totalPages={data.reviews.totalPages} onChange={setPage} />}
      </div>

      <Modal open={open} onClose={() => setOpen(false)} title="ثبت دیدگاه">
        <ReviewForm productId={productId} onDone={() => setOpen(false)} />
      </Modal>
    </div>
  )
}
