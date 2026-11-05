import { lazy, Suspense, type ReactNode } from 'react'
import { createBrowserRouter } from 'react-router'
import { PageLoader } from '@/components/ui'
import { RequireAuth, StoreLayout } from '@/components/layout/StoreLayout'
import HomePage from '@/pages/HomePage'
import ProductListPage from '@/pages/ProductListPage'
import ProductPage from '@/pages/ProductPage'
import CartPage from '@/pages/CartPage'
import { LoginPage, RegisterPage } from '@/pages/AuthPages'
import { CategoriesPage, NotFoundPage } from '@/pages/MiscPages'

const CheckoutPage = lazy(() => import('@/pages/CheckoutPage'))
const GatewayPage = lazy(() => import('@/pages/PaymentPages').then((m) => ({ default: m.GatewayPage })))
const PaymentResultPage = lazy(() => import('@/pages/PaymentPages').then((m) => ({ default: m.PaymentResultPage })))
const profile = () => import('@/pages/profile/ProfilePages')
const ProfileLayout = lazy(() => profile().then((m) => ({ default: m.ProfileLayout })))
const ProfileHome = lazy(() => profile().then((m) => ({ default: m.ProfileHome })))
const OrdersPage = lazy(() => profile().then((m) => ({ default: m.OrdersPage })))
const OrderDetailPage = lazy(() => profile().then((m) => ({ default: m.OrderDetailPage })))
const WishlistPage = lazy(() => profile().then((m) => ({ default: m.WishlistPage })))
const AddressesPage = lazy(() => profile().then((m) => ({ default: m.AddressesPage })))
const AccountPage = lazy(() => profile().then((m) => ({ default: m.AccountPage })))

// Admin panel is a separate chunk, loaded only for admins.
const AdminLayout = lazy(() => import('@/pages/admin/AdminLayout'))
const AdminDashboard = lazy(() => import('@/pages/admin/DashboardPage'))
const AdminProducts = lazy(() => import('@/pages/admin/ProductsPage'))
const AdminProductEdit = lazy(() => import('@/pages/admin/ProductEditPage'))
const admin = () => import('@/pages/admin/CatalogPages')
const AdminCategories = lazy(() => admin().then((m) => ({ default: m.CategoriesPage })))
const AdminBrands = lazy(() => admin().then((m) => ({ default: m.BrandsPage })))
const AdminBanners = lazy(() => admin().then((m) => ({ default: m.BannersPage })))
const sales = () => import('@/pages/admin/SalesPages')
const AdminOrders = lazy(() => sales().then((m) => ({ default: m.OrdersPage })))
const AdminOrderDetail = lazy(() => sales().then((m) => ({ default: m.OrderDetailPage })))
const AdminUsers = lazy(() => sales().then((m) => ({ default: m.UsersPage })))
const AdminReviews = lazy(() => sales().then((m) => ({ default: m.ReviewsPage })))
const AdminCoupons = lazy(() => sales().then((m) => ({ default: m.CouponsPage })))

const s = (node: ReactNode) => <Suspense fallback={<PageLoader />}>{node}</Suspense>

export const router = createBrowserRouter([
  {
    element: <StoreLayout />,
    children: [
      { index: true, element: <HomePage /> },
      { path: 'category/:slug', element: <ProductListPage /> },
      { path: 'search', element: <ProductListPage /> },
      { path: 'categories', element: <CategoriesPage /> },
      { path: 'product/:slug', element: <ProductPage /> },
      { path: 'cart', element: <CartPage /> },
      { path: 'checkout', element: <RequireAuth>{s(<CheckoutPage />)}</RequireAuth> },
      { path: 'payment/result', element: s(<PaymentResultPage />) },
      {
        path: 'profile',
        element: <RequireAuth>{s(<ProfileLayout />)}</RequireAuth>,
        children: [
          { index: true, element: s(<ProfileHome />) },
          { path: 'orders', element: s(<OrdersPage />) },
          { path: 'orders/:id', element: s(<OrderDetailPage />) },
          { path: 'wishlist', element: s(<WishlistPage />) },
          { path: 'addresses', element: s(<AddressesPage />) },
          { path: 'account', element: s(<AccountPage />) },
        ],
      },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
  { path: 'login', element: <LoginPage /> },
  { path: 'register', element: <RegisterPage /> },
  { path: 'payment/gateway/:token', element: s(<GatewayPage />) },
  {
    path: 'admin',
    element: <RequireAuth admin>{s(<AdminLayout />)}</RequireAuth>,
    children: [
      { index: true, element: s(<AdminDashboard />) },
      { path: 'products', element: s(<AdminProducts />) },
      { path: 'products/new', element: s(<AdminProductEdit />) },
      { path: 'products/:id', element: s(<AdminProductEdit />) },
      { path: 'categories', element: s(<AdminCategories />) },
      { path: 'brands', element: s(<AdminBrands />) },
      { path: 'banners', element: s(<AdminBanners />) },
      { path: 'orders', element: s(<AdminOrders />) },
      { path: 'orders/:id', element: s(<AdminOrderDetail />) },
      { path: 'users', element: s(<AdminUsers />) },
      { path: 'reviews', element: s(<AdminReviews />) },
      { path: 'coupons', element: s(<AdminCoupons />) },
    ],
  },
])
