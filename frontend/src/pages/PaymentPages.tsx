import { useMutation, useQuery } from '@tanstack/react-query'
import { CheckCircle2, CreditCard, Lock, XCircle } from 'lucide-react'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { Link, useLocation, useNavigate, useParams } from 'react-router'
import { errorMessage, get, post } from '@/lib/api'
import { formatNumber, toEnDigits, toFaDigits } from '@/lib/format'
import type { GatewayInfo, PaymentResult } from '@/lib/types'
import { PageLoader, Spinner } from '@/components/ui'

/** Simulated bank payment page (looks like an Iranian Shaparak gateway). */
export function GatewayPage() {
  const { token = '' } = useParams()
  const navigate = useNavigate()
  const [card, setCard] = useState('6037 9900 0000 1234')
  const [cvv, setCvv] = useState('123')
  const [otp, setOtp] = useState('')
  const { data, isLoading, isError } = useQuery({ queryKey: ['gateway', token], queryFn: () => get<GatewayInfo>(`/payments/${token}`) })

  const complete = useMutation({
    mutationFn: (success: boolean) => post<PaymentResult>(`/payments/${token}/complete`, { success, cardNumber: toEnDigits(card) }),
    onSuccess: (r) => navigate('/payment/result', { state: r, replace: true }),
    onError: (e) => toast.error(errorMessage(e)),
  })

  if (isLoading) return <PageLoader />
  if (isError || !data) return <div className="py-20 text-center">تراکنش نامعتبر است.</div>

  return (
    <div className="min-h-screen bg-gradient-to-b from-slate-100 to-slate-200 py-8">
      <div className="mx-auto max-w-3xl px-4">
        <div className="mb-4 flex items-center justify-between rounded-xl bg-[#0b3d6e] px-5 py-4 text-white">
          <div className="flex items-center gap-2"><Lock size={18} /> درگاه پرداخت اینترنتی (شبیه‌سازی)</div>
          <span className="text-xs opacity-80">نسخه نمایشی — مبلغی کسر نمی‌شود</span>
        </div>
        {data.status !== 'Pending' ? (
          <div className="card p-8 text-center">
            <p className="mb-4">این تراکنش قبلاً پردازش شده است.</p>
            <Link to="/profile/orders" className="btn-primary">مشاهده سفارش‌ها</Link>
          </div>
        ) : (
          <div className="grid gap-4 md:grid-cols-[1fr_260px]">
            <form className="card space-y-4 p-6" onSubmit={(e) => { e.preventDefault(); complete.mutate(true) }}>
              <h2 className="flex items-center gap-2 font-bold"><CreditCard size={20} /> اطلاعات کارت</h2>
              <label className="block"><span className="label">شماره کارت</span>
                <input className="input text-center tracking-widest" dir="ltr" value={card} onChange={(e) => setCard(e.target.value)} required minLength={16} />
              </label>
              <div className="grid grid-cols-2 gap-3">
                <label className="block"><span className="label">CVV2</span>
                  <input className="input text-center" dir="ltr" value={cvv} onChange={(e) => setCvv(e.target.value)} maxLength={4} required />
                </label>
                <label className="block"><span className="label">تاریخ انقضا</span>
                  <input className="input text-center" dir="ltr" defaultValue="08/12" required />
                </label>
              </div>
              <label className="block"><span className="label">رمز دوم (هر عددی وارد کنید)</span>
                <input className="input text-center" dir="ltr" type="password" value={otp} onChange={(e) => setOtp(e.target.value)} placeholder="••••••" />
              </label>
              <div className="flex gap-3 pt-2">
                <button className="btn flex-1 bg-emerald-600 py-3 text-white hover:bg-emerald-700" disabled={complete.isPending}>
                  {complete.isPending ? <Spinner className="size-5 text-white" /> : 'پرداخت'}
                </button>
                <button type="button" className="btn flex-1 bg-gray-200 py-3 hover:bg-gray-300" disabled={complete.isPending} onClick={() => complete.mutate(false)}>
                  انصراف (پرداخت ناموفق)
                </button>
              </div>
            </form>
            <div className="card h-fit space-y-4 p-6 text-sm">
              <div><div className="text-muted">پذیرنده</div><div className="font-bold">{data.merchantName}</div></div>
              <div><div className="text-muted">شماره سفارش</div><div className="font-bold" dir="ltr">{data.orderNumber}</div></div>
              <div><div className="text-muted">مبلغ قابل پرداخت</div><div className="text-lg font-black text-emerald-700">{formatNumber(data.amount)} تومان</div></div>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

export function PaymentResultPage() {
  const result = useLocation().state as PaymentResult | null
  if (!result) return <div className="container-x py-20 text-center"><Link to="/" className="btn-primary">بازگشت به فروشگاه</Link></div>
  return (
    <div className="container-x py-10">
      <div className="card mx-auto max-w-xl p-8 text-center">
        {result.success ? <CheckCircle2 size={80} className="mx-auto mb-4 text-success" /> : <XCircle size={80} className="mx-auto mb-4 text-brand-500" />}
        <h1 className={`mb-2 text-xl font-bold ${result.success ? 'text-success' : 'text-brand-500'}`}>
          {result.success ? 'سفارش شما با موفقیت ثبت شد' : 'پرداخت ناموفق بود'}
        </h1>
        <p className="mb-6 text-sm leading-7 text-muted">{result.message}</p>
        <div className="mb-6 divide-y divide-line rounded-xl bg-surface text-sm">
          <div className="flex justify-between p-3"><span className="text-muted">کد سفارش</span><b dir="ltr">{result.orderNumber}</b></div>
          <div className="flex justify-between p-3"><span className="text-muted">مبلغ</span><b>{formatNumber(result.amount)} تومان</b></div>
          {result.referenceId && <div className="flex justify-between p-3"><span className="text-muted">کد پیگیری</span><b>{toFaDigits(result.referenceId)}</b></div>}
        </div>
        <div className="flex justify-center gap-3">
          <Link to={`/profile/orders/${result.orderId}`} className="btn-primary">{result.success ? 'پیگیری سفارش' : 'پرداخت مجدد'}</Link>
          <Link to="/" className="btn-outline">بازگشت به فروشگاه</Link>
        </div>
      </div>
    </div>
  )
}
