import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { errorMessage, post, put } from '@/lib/api'
import { toEnDigits } from '@/lib/format'
import type { Address, AddressInput } from '@/lib/types'
import { Field } from '@/components/ui'

const provinces = ['تهران', 'البرز', 'اصفهان', 'خراسان رضوی', 'فارس', 'آذربایجان شرقی', 'خوزستان', 'مازندران', 'گیلان', 'کرمان', 'یزد', 'قم', 'سایر']

export function AddressForm({ address, defaultName, defaultPhone, onSaved }: {
  address?: Address
  defaultName?: string
  defaultPhone?: string
  onSaved: (a: Address) => void
}) {
  const [form, setForm] = useState<AddressInput>({
    title: address?.title ?? 'خانه',
    receiverName: address?.receiverName ?? defaultName ?? '',
    receiverPhone: address?.receiverPhone ?? defaultPhone ?? '',
    province: address?.province ?? 'تهران',
    city: address?.city ?? '',
    fullAddress: address?.fullAddress ?? '',
    postalCode: address?.postalCode ?? '',
    isDefault: address?.isDefault ?? false,
  })
  const set = <K extends keyof AddressInput>(k: K, v: AddressInput[K]) => setForm((f) => ({ ...f, [k]: v }))

  const mutation = useMutation({
    mutationFn: () => {
      const body = { ...form, receiverPhone: toEnDigits(form.receiverPhone), postalCode: toEnDigits(form.postalCode) }
      return address ? put<Address>(`/addresses/${address.id}`, body) : post<Address>('/addresses', body)
    },
    onSuccess: (a) => { toast.success('آدرس ذخیره شد'); onSaved(a) },
    onError: (e) => toast.error(errorMessage(e)),
  })

  return (
    <form className="grid gap-4 sm:grid-cols-2" onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
      <Field label="نشانی پستی" className="sm:col-span-2">
        <textarea className="input min-h-24" value={form.fullAddress} onChange={(e) => set('fullAddress', e.target.value)} required
          placeholder="خیابان، کوچه، پلاک، واحد" />
      </Field>
      <Field label="استان">
        <select className="input" value={form.province} onChange={(e) => set('province', e.target.value)}>
          {provinces.map((p) => <option key={p}>{p}</option>)}
        </select>
      </Field>
      <Field label="شهر"><input className="input" value={form.city} onChange={(e) => set('city', e.target.value)} required /></Field>
      <Field label="کد پستی (۱۰ رقم)"><input className="input text-left" dir="ltr" inputMode="numeric" maxLength={10} value={form.postalCode} onChange={(e) => set('postalCode', e.target.value)} required /></Field>
      <Field label="عنوان آدرس"><input className="input" value={form.title} onChange={(e) => set('title', e.target.value)} required placeholder="خانه، محل کار..." /></Field>
      <Field label="نام و نام خانوادگی گیرنده"><input className="input" value={form.receiverName} onChange={(e) => set('receiverName', e.target.value)} required /></Field>
      <Field label="شماره موبایل گیرنده"><input className="input text-left" dir="ltr" inputMode="tel" maxLength={11} value={form.receiverPhone} onChange={(e) => set('receiverPhone', e.target.value)} required placeholder="09xxxxxxxxx" /></Field>
      <label className="flex items-center gap-2 text-sm sm:col-span-2">
        <input type="checkbox" className="size-4 accent-brand-500" checked={form.isDefault} onChange={(e) => set('isDefault', e.target.checked)} />
        آدرس پیش‌فرض من باشد
      </label>
      <button className="btn-primary sm:col-span-2" disabled={mutation.isPending}>ثبت آدرس</button>
    </form>
  )
}
