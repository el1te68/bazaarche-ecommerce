import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { clsx } from 'clsx'
import { MapPin, Pencil, Plus, TicketPercent, Truck, Zap } from 'lucide-react'
import { useEffect, useState } from 'react'
import toast from 'react-hot-toast'
import { Link, Navigate, useNavigate } from 'react-router'
import { errorMessage, get, post } from '@/lib/api'
import { formatNumber, img, toFaDigits } from '@/lib/format'
import type { Address, CheckoutPreview, CheckoutResult, ShippingMethod } from '@/lib/types'
import { useCart } from '@/hooks/useCart'
import { useAuthStore } from '@/stores/auth'
import { Modal, PageLoader, Price, Spinner } from '@/components/ui'
import { AddressForm } from '@/components/AddressForm'

export default function CheckoutPage() {
  const user = useAuthStore((s) => s.user)!
  const qc = useQueryClient()
  const navigate = useNavigate()
  const { cart, isLoading: cartLoading } = useCart()
  const [addressId, setAddressId] = useState<number>()
  const [shipping, setShipping] = useState<ShippingMethod>('Standard')
  const [couponInput, setCouponInput] = useState('')
  const [coupon, setCoupon] = useState<string>()
  const [addressModal, setAddressModal] = useState<{ open: boolean; address?: Address }>({ open: false })
  const [chooseOpen, setChooseOpen] = useState(false)

  const { data: addresses = [], isLoading: addrLoading } = useQuery({ queryKey: ['addresses'], queryFn: () => get<Address[]>('/addresses') })
  useEffect(() => {
    if (!addressId && addresses.length) setAddressId((addresses.find((a) => a.isDefault) ?? addresses[0]).id)
  }, [addresses, addressId])

  const { data: preview } = useQuery({
    queryKey: ['checkout-preview', shipping, coupon, cart.payableTotal],
    queryFn: () => post<CheckoutPreview>('/checkout/preview', { shippingMethod: shipping, couponCode: coupon }),
    placeholderData: keepPreviousData,
  })

  useEffect(() => {
    if (coupon && preview && !preview.couponApplied && preview.couponMessage) {
      toast.error(preview.couponMessage)
      setCoupon(undefined)
    }
  }, [preview, coupon])

  const checkout = useMutation({
    mutationFn: () => post<CheckoutResult>('/checkout', { addressId, shippingMethod: shipping, couponCode: coupon }),
    onSuccess: (r) => {
      qc.invalidateQueries({ queryKey: ['cart'] })
      navigate(`/payment/gateway/${r.paymentToken}`)
    },
    onError: (e) => toast.error(errorMessage(e)),
  })

  if (cartLoading || addrLoading) return <PageLoader />
  if (cart.items.length === 0 && !checkout.isSuccess) return <Navigate to="/cart" replace />

  const address = addresses.find((a) => a.id === addressId)

  return (
    <div className="container-x grid gap-4 py-6 lg:grid-cols-[1fr_340px]">
      <div className="space-y-4">
        {/* Address */}
        <section className="card p-5">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="flex items-center gap-2 font-bold"><MapPin size={20} /> آدرس تحویل سفارش</h2>
            {addresses.length > 0 && (
              <button onClick={() => setChooseOpen(true)} className="flex items-center gap-1 text-sm text-sky-600">تغییر یا ویرایش آدرس</button>
            )}
          </div>
          {address ? (
            <div className="space-y-2 text-sm">
              <p className="font-bold leading-7">{address.fullAddress}</p>
              <p className="text-muted">{address.province}، {address.city} • کد پستی {toFaDigits(address.postalCode)}</p>
              <p className="text-muted">{address.receiverName} • {toFaDigits(address.receiverPhone)}</p>
            </div>
          ) : (
            <button onClick={() => setAddressModal({ open: true })} className="btn-outline"><Plus size={18} /> افزودن آدرس جدید</button>
          )}
        </section>

        {/* Shipping */}
        <section className="card p-5">
          <h2 className="mb-4 flex items-center gap-2 font-bold"><Truck size={20} /> شیوه ارسال</h2>
          <div className="grid gap-3 sm:grid-cols-2">
            {(preview?.shippingOptions ?? []).map((o) => (
              <button key={o.method} onClick={() => setShipping(o.method)}
                className={clsx('flex items-start gap-3 rounded-xl border-2 p-4 text-right', shipping === o.method ? 'border-brand-500 bg-brand-50/50' : 'border-line')}>
                {o.method === 'Express' ? <Zap className="text-amber-500" /> : <Truck className="text-sky-600" />}
                <div className="flex-1">
                  <div className="font-bold">{o.title}</div>
                  <div className="text-xs text-muted">{o.description}</div>
                </div>
                <div className="text-sm font-bold">{o.cost === 0 ? <span className="text-success">رایگان</span> : `${formatNumber(o.cost)} تومان`}</div>
              </button>
            ))}
          </div>
          <div className="scrollbar-none mt-5 flex gap-3 overflow-x-auto">
            {cart.items.map((i) => (
              <div key={i.variantId} className="relative shrink-0">
                <img src={img(i.imageUrl)} alt={i.title} title={i.title} className="size-20 rounded-lg border border-line object-contain" />
                {i.quantity > 1 && <span className="absolute bottom-1 left-1 rounded bg-ink px-1 text-[10px] text-white">{toFaDigits(i.quantity)}</span>}
              </div>
            ))}
          </div>
        </section>

        {/* Coupon */}
        <section className="card p-5">
          <h2 className="mb-4 flex items-center gap-2 font-bold"><TicketPercent size={20} /> کد تخفیف</h2>
          {coupon && preview?.couponApplied ? (
            <div className="flex items-center justify-between rounded-lg bg-emerald-50 p-3 text-sm text-emerald-700">
              <span>{preview.couponMessage}</span>
              <button className="text-xs underline" onClick={() => { setCoupon(undefined); setCouponInput('') }}>حذف</button>
            </div>
          ) : (
            <form className="flex gap-2" onSubmit={(e) => { e.preventDefault(); if (couponInput.trim()) setCoupon(couponInput.trim()) }}>
              <input className="input max-w-xs text-left uppercase" dir="ltr" placeholder="WELCOME" value={couponInput} onChange={(e) => setCouponInput(e.target.value)} />
              <button className="btn-outline">اعمال</button>
            </form>
          )}
          <p className="mt-2 text-xs text-muted">برای تست: <b dir="ltr">WELCOME</b> (۱۰٪ تا سقف ۵۰۰ هزار تومان) یا <b dir="ltr">BAZAAR50</b></p>
        </section>
      </div>

      <aside className="h-fit space-y-4 lg:sticky lg:top-36">
        <div className="card space-y-4 p-5 text-sm">
          {preview ? (
            <>
              <div className="flex justify-between text-muted"><span>قیمت کالاها ({toFaDigits(cart.totalQuantity)})</span><span>{formatNumber(preview.itemsTotal)} تومان</span></div>
              {preview.productsDiscount > 0 && <div className="flex justify-between text-brand-500"><span>تخفیف کالاها</span><span>{formatNumber(preview.productsDiscount)}-</span></div>}
              <div className="flex justify-between text-muted"><span>هزینه ارسال</span><span>{preview.shippingCost === 0 ? 'رایگان' : `${formatNumber(preview.shippingCost)} تومان`}</span></div>
              {preview.couponDiscount > 0 && <div className="flex justify-between text-brand-500"><span>کد تخفیف</span><span>{formatNumber(preview.couponDiscount)}-</span></div>}
              <div className="flex justify-between border-t border-line pt-4 font-bold"><span>مبلغ قابل پرداخت</span><Price value={preview.payable} /></div>
            </>
          ) : <Spinner />}
          <button className="btn-primary w-full py-3" disabled={!address || checkout.isPending} onClick={() => checkout.mutate()}>
            {checkout.isPending ? <Spinner className="size-5 text-white" /> : 'پرداخت'}
          </button>
          {!address && <p className="text-xs text-brand-600">برای ادامه، آدرس تحویل را وارد کنید.</p>}
        </div>
        <Link to="/cart" className="block px-2 text-center text-xs text-sky-600">بازگشت به سبد خرید</Link>
      </aside>

      <Modal open={chooseOpen} onClose={() => setChooseOpen(false)} title="انتخاب آدرس">
        <div className="space-y-3">
          {addresses.map((a) => (
            <div key={a.id} className={clsx('flex items-start gap-3 rounded-xl border-2 p-4', a.id === addressId ? 'border-brand-500' : 'border-line')}>
              <button className="flex-1 text-right text-sm" onClick={() => { setAddressId(a.id); setChooseOpen(false) }}>
                <div className="mb-1 font-bold">{a.title}</div>
                <div className="leading-6">{a.fullAddress}</div>
                <div className="text-xs text-muted">{a.receiverName}</div>
              </button>
              <button className="text-muted" onClick={() => { setChooseOpen(false); setAddressModal({ open: true, address: a }) }} aria-label="ویرایش"><Pencil size={16} /></button>
            </div>
          ))}
          <button className="btn-outline w-full" onClick={() => { setChooseOpen(false); setAddressModal({ open: true }) }}><Plus size={18} /> افزودن آدرس جدید</button>
        </div>
      </Modal>

      <Modal open={addressModal.open} onClose={() => setAddressModal({ open: false })} title={addressModal.address ? 'ویرایش آدرس' : 'آدرس جدید'} wide>
        <AddressForm address={addressModal.address} defaultName={user.fullName} defaultPhone={user.phoneNumber}
          onSaved={(a) => { qc.invalidateQueries({ queryKey: ['addresses'] }); setAddressId(a.id); setAddressModal({ open: false }) }} />
      </Modal>
    </div>
  )
}
