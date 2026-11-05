import type { OrderStatus } from './types'

const numberFormat = new Intl.NumberFormat('fa-IR')
const dateFormat = new Intl.DateTimeFormat('fa-IR-u-ca-persian', { year: 'numeric', month: 'long', day: 'numeric' })
const dateTimeFormat = new Intl.DateTimeFormat('fa-IR-u-ca-persian', {
  year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute: '2-digit',
})
const shortDate = new Intl.DateTimeFormat('fa-IR-u-ca-persian', { month: 'short', day: 'numeric' })

export const toFaDigits = (value: string | number) =>
  String(value).replace(/\d/g, (d) => '۰۱۲۳۴۵۶۷۸۹'[Number(d)])

export const toEnDigits = (value: string) =>
  value.replace(/[۰-۹]/g, (d) => String('۰۱۲۳۴۵۶۷۸۹'.indexOf(d))).replace(/[٠-٩]/g, (d) => String('٠١٢٣٤٥٦٧٨٩'.indexOf(d)))

export const formatNumber = (n: number) => numberFormat.format(n)
export const formatPrice = (n: number) => numberFormat.format(n)
export const formatDate = (iso: string) => dateFormat.format(new Date(iso))
export const formatDateTime = (iso: string) => dateTimeFormat.format(new Date(iso))
export const formatShortDate = (iso: string) => shortDate.format(new Date(iso))

export const orderStatusLabel: Record<OrderStatus, string> = {
  PendingPayment: 'در انتظار پرداخت',
  Processing: 'در حال پردازش',
  Shipped: 'ارسال شده',
  Delivered: 'تحویل شده',
  Cancelled: 'لغو شده',
}

export const orderStatusColor: Record<OrderStatus, string> = {
  PendingPayment: 'bg-amber-50 text-amber-700 ring-amber-200',
  Processing: 'bg-sky-50 text-sky-700 ring-sky-200',
  Shipped: 'bg-indigo-50 text-indigo-700 ring-indigo-200',
  Delivered: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Cancelled: 'bg-rose-50 text-rose-700 ring-rose-200',
}

/** Image URLs from the API are relative (/uploads/...). */
export const img = (url?: string | null) => url || '/favicon.svg'
