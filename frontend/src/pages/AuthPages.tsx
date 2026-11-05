import { useMutation, useQueryClient } from '@tanstack/react-query'
import { Eye, EyeOff } from 'lucide-react'
import { useState } from 'react'
import toast from 'react-hot-toast'
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router'
import { errorMessage, post } from '@/lib/api'
import { toEnDigits } from '@/lib/format'
import type { AuthResponse } from '@/lib/types'
import { mergeGuestCart } from '@/hooks/useCart'
import { useAuthStore } from '@/stores/auth'
import { Field, Spinner } from '@/components/ui'
import { Logo } from '@/components/layout/Logo'

function useAuthSuccess() {
  const setAuth = useAuthStore((s) => s.setAuth)
  const qc = useQueryClient()
  const navigate = useNavigate()
  const [params] = useSearchParams()
  return async (r: AuthResponse, welcome: string) => {
    setAuth(r.token, r.user)
    await mergeGuestCart()
    qc.invalidateQueries()
    toast.success(welcome)
    const returnUrl = params.get('returnUrl')
    navigate(returnUrl && returnUrl.startsWith('/') ? returnUrl : r.user.role === 'Admin' ? '/admin' : '/', { replace: true })
  }
}

function AuthShell({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen items-start justify-center bg-surface px-4 py-10 sm:items-center">
      <div className="w-full max-w-md rounded-2xl border border-line bg-white p-8">
        <div className="mb-6 flex justify-center"><Logo /></div>
        <h1 className="mb-6 text-lg font-bold">{title}</h1>
        {children}
      </div>
    </div>
  )
}

function PasswordInput({ value, onChange }: { value: string; onChange: (v: string) => void }) {
  const [show, setShow] = useState(false)
  return (
    <div className="relative">
      <input className="input pl-10" type={show ? 'text' : 'password'} value={value} onChange={(e) => onChange(e.target.value)} required dir="ltr" />
      <button type="button" className="absolute left-3 top-1/2 -translate-y-1/2 text-muted" onClick={() => setShow(!show)} tabIndex={-1}>
        {show ? <EyeOff size={18} /> : <Eye size={18} />}
      </button>
    </div>
  )
}

export function LoginPage() {
  const user = useAuthStore((s) => s.user)
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [params] = useSearchParams()
  const onSuccess = useAuthSuccess()
  const mutation = useMutation({
    mutationFn: () => post<AuthResponse>('/auth/login', { username: toEnDigits(username.trim()), password }),
    onSuccess: (r) => onSuccess(r, `${r.user.fullName} عزیز، خوش آمدید`),
    onError: (e) => toast.error(errorMessage(e)),
  })
  if (user && !mutation.isSuccess) return <Navigate to="/" replace />

  return (
    <AuthShell title="ورود به حساب کاربری">
      <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <Field label="شماره موبایل یا ایمیل">
          <input className="input text-left" dir="ltr" value={username} onChange={(e) => setUsername(e.target.value)} required autoFocus placeholder="09xxxxxxxxx" />
        </Field>
        <Field label="رمز عبور"><PasswordInput value={password} onChange={setPassword} /></Field>
        <button className="btn-primary w-full py-3" disabled={mutation.isPending}>{mutation.isPending ? <Spinner className="size-5 text-white" /> : 'ورود'}</button>
      </form>
      <p className="mt-6 text-center text-sm">
        حساب کاربری ندارید؟ <Link to={`/register?${params}`} className="font-bold text-sky-600">ثبت‌نام کنید</Link>
      </p>
      <div className="mt-6 space-y-1 rounded-xl bg-surface p-4 text-xs leading-6 text-ink/80">
        <b>حساب‌های نمایشی:</b>
        <button type="button" className="block text-right hover:text-sky-600" onClick={() => { setUsername('09121111111'); setPassword('Demo@123') }}>
          مشتری: <span dir="ltr">09121111111 / Demo@123</span>
        </button>
        <button type="button" className="block text-right hover:text-sky-600" onClick={() => { setUsername('09120000000'); setPassword('Admin@123') }}>
          مدیر: <span dir="ltr">09120000000 / Admin@123</span>
        </button>
      </div>
    </AuthShell>
  )
}

export function RegisterPage() {
  const user = useAuthStore((s) => s.user)
  const [form, setForm] = useState({ fullName: '', phoneNumber: '', email: '', password: '' })
  const onSuccess = useAuthSuccess()
  const mutation = useMutation({
    mutationFn: () => post<AuthResponse>('/auth/register', { ...form, phoneNumber: toEnDigits(form.phoneNumber), email: form.email || null }),
    onSuccess: (r) => onSuccess(r, 'ثبت‌نام با موفقیت انجام شد'),
    onError: (e) => toast.error(errorMessage(e)),
  })
  if (user && !mutation.isSuccess) return <Navigate to="/" replace />
  const set = (k: keyof typeof form) => (e: React.ChangeEvent<HTMLInputElement>) => setForm({ ...form, [k]: e.target.value })

  return (
    <AuthShell title="ثبت‌نام در بازارچه">
      <form className="space-y-4" onSubmit={(e) => { e.preventDefault(); mutation.mutate() }}>
        <Field label="نام و نام خانوادگی"><input className="input" value={form.fullName} onChange={set('fullName')} required minLength={3} /></Field>
        <Field label="شماره موبایل"><input className="input text-left" dir="ltr" inputMode="tel" maxLength={11} value={form.phoneNumber} onChange={set('phoneNumber')} required placeholder="09xxxxxxxxx" /></Field>
        <Field label="ایمیل (اختیاری)"><input className="input text-left" dir="ltr" type="email" value={form.email} onChange={set('email')} /></Field>
        <Field label="رمز عبور (حداقل ۶ کاراکتر)"><PasswordInput value={form.password} onChange={(v) => setForm({ ...form, password: v })} /></Field>
        <button className="btn-primary w-full py-3" disabled={mutation.isPending}>{mutation.isPending ? <Spinner className="size-5 text-white" /> : 'ثبت‌نام'}</button>
      </form>
      <p className="mt-6 text-center text-sm">قبلاً ثبت‌نام کرده‌اید؟ <Link to="/login" className="font-bold text-sky-600">وارد شوید</Link></p>
    </AuthShell>
  )
}
