import { Navigate, Outlet, ScrollRestoration, useLocation } from 'react-router'
import { useAuthStore } from '@/stores/auth'
import { Footer } from './Footer'
import { Header } from './Header'
import { MobileNav } from './MobileNav'

export function StoreLayout() {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1">
        <Outlet />
      </main>
      <Footer />
      <MobileNav />
      <ScrollRestoration />
    </div>
  )
}

export function RequireAuth({ admin, children }: { admin?: boolean; children: React.ReactNode }) {
  const user = useAuthStore((s) => s.user)
  const location = useLocation()
  if (!user) return <Navigate to={`/login?returnUrl=${encodeURIComponent(location.pathname + location.search)}`} replace />
  if (admin && user.role !== 'Admin') return <Navigate to="/" replace />
  return <>{children}</>
}
