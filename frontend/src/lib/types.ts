// Types mirroring the .NET API DTOs.

export type UserRole = 'Customer' | 'Admin'
export type OrderStatus = 'PendingPayment' | 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled'
export type PaymentStatus = 'Pending' | 'Succeeded' | 'Failed'
export type ShippingMethod = 'Standard' | 'Express'
export type CouponType = 'Percent' | 'Fixed'
export type BannerPosition = 'HeroSlider' | 'MiddleBanner'

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface User {
  id: number
  fullName: string
  phoneNumber: string
  email?: string | null
  role: UserRole
  createdAt: string
}

export interface AuthResponse {
  token: string
  expiresAt: string
  user: User
}

export interface ProductCard {
  id: number
  title: string
  slug: string
  imageUrl?: string | null
  price: number
  finalPrice: number
  discountPercent: number
  rating: number
  reviewCount: number
  inStock: boolean
  isAmazingOffer: boolean
  offerEndsAt?: string | null
  colors: string[]
}

export interface CategoryNode {
  id: number
  name: string
  slug: string
  icon?: string | null
  imageUrl?: string | null
  children: CategoryNode[]
}

export interface CategoryLink {
  id: number
  name: string
  slug: string
}

export interface Brand {
  id: number
  name: string
  englishName: string
  slug: string
  logoUrl?: string | null
}

export interface Banner {
  id: number
  title: string
  imageUrl: string
  linkUrl?: string | null
  position: BannerPosition
}

export interface HomeData {
  heroBanners: Banner[]
  middleBanners: Banner[]
  categories: CategoryNode[]
  amazingOffers: ProductCard[]
  bestsellers: ProductCard[]
  newest: ProductCard[]
  topDiscounts: ProductCard[]
  topRated: ProductCard[]
  brands: Brand[]
}

export interface Variant {
  id: number
  colorName?: string | null
  colorHex?: string | null
  price: number
  finalPrice: number
  discountPercent: number
  stock: number
}

export interface Specification {
  key: string
  value: string
}

export interface ProductDetail {
  id: number
  title: string
  englishTitle?: string | null
  slug: string
  description: string
  breadcrumbs: CategoryLink[]
  brand?: Brand | null
  images: string[]
  variants: Variant[]
  specifications: Specification[]
  rating: number
  reviewCount: number
  soldCount: number
  isAmazingOffer: boolean
  offerEndsAt?: string | null
  isInWishlist: boolean
}

export interface BrandFacet {
  id: number
  name: string
  slug: string
  count: number
}

export interface ProductListResponse {
  products: PagedResult<ProductCard>
  category?: CategoryNode | null
  breadcrumbs: CategoryLink[]
  brands: BrandFacet[]
  maxPrice: number
}

export interface Suggestion {
  products: { title: string; slug: string; imageUrl?: string | null }[]
  categories: CategoryLink[]
}

export interface Review {
  id: number
  userName: string
  rating: number
  title: string
  comment: string
  createdAt: string
  isBuyer: boolean
}

export interface ReviewSummary {
  average: number
  count: number
  distribution: Record<string, number>
  reviews: PagedResult<Review>
}

export interface CartLine {
  variantId: number
  quantity: number
}

export interface CartItem {
  variantId: number
  productId: number
  title: string
  slug: string
  imageUrl?: string | null
  colorName?: string | null
  colorHex?: string | null
  price: number
  finalPrice: number
  quantity: number
  stock: number
  maxQuantity: number
}

export interface Cart {
  items: CartItem[]
  totalQuantity: number
  itemsTotal: number
  productsDiscount: number
  payableTotal: number
}

export interface Address {
  id: number
  title: string
  receiverName: string
  receiverPhone: string
  province: string
  city: string
  fullAddress: string
  postalCode: string
  isDefault: boolean
}

export type AddressInput = Omit<Address, 'id'>

export interface ShippingOption {
  method: ShippingMethod
  title: string
  description: string
  cost: number
}

export interface CheckoutPreview {
  itemsTotal: number
  productsDiscount: number
  shippingCost: number
  couponDiscount: number
  payable: number
  couponMessage?: string | null
  couponApplied: boolean
  shippingOptions: ShippingOption[]
}

export interface CheckoutResult {
  orderId: number
  orderNumber: string
  paymentToken: string
  amount: number
}

export interface OrderSummary {
  id: number
  orderNumber: string
  status: OrderStatus
  createdAt: string
  payableAmount: number
  itemsCount: number
  images: (string | null)[]
}

export interface OrderItem {
  productId: number
  variantId: number
  title: string
  slug: string
  imageUrl?: string | null
  colorName?: string | null
  unitPrice: number
  unitFinalPrice: number
  quantity: number
}

export interface Payment {
  id: number
  token: string
  amount: number
  status: PaymentStatus
  referenceId?: string | null
  cardMask?: string | null
  createdAt: string
  completedAt?: string | null
}

export interface OrderDetail {
  id: number
  orderNumber: string
  status: OrderStatus
  createdAt: string
  paidAt?: string | null
  receiverName: string
  receiverPhone: string
  province: string
  city: string
  fullAddress: string
  postalCode: string
  shippingMethod: ShippingMethod
  shippingCost: number
  itemsTotal: number
  productsDiscount: number
  couponCode?: string | null
  couponDiscount: number
  payableAmount: number
  trackingCode?: string | null
  items: OrderItem[]
  payments: Payment[]
  customerName?: string | null
  customerPhone?: string | null
}

export interface GatewayInfo {
  token: string
  orderNumber: string
  amount: number
  merchantName: string
  status: PaymentStatus
  createdAt: string
}

export interface PaymentResult {
  success: boolean
  orderId: number
  orderNumber: string
  amount: number
  referenceId?: string | null
  message: string
}

// ---------- Admin ----------

export interface Dashboard {
  totalRevenue: number
  totalOrders: number
  totalCustomers: number
  totalProducts: number
  todayRevenue: number
  todayOrders: number
  pendingReviews: number
  sales: { date: string; revenue: number; orders: number }[]
  orderStatuses: { status: OrderStatus; count: number }[]
  recentOrders: { id: number; orderNumber: string; customerName: string; amount: number; status: OrderStatus; createdAt: string }[]
  lowStock: { id: number; title: string; slug: string; stock: number; imageUrl?: string | null }[]
  topProducts: { id: number; title: string; slug: string; soldCount: number; imageUrl?: string | null }[]
}

export interface AdminProductRow {
  id: number
  title: string
  slug: string
  imageUrl?: string | null
  categoryName: string
  brandName?: string | null
  finalPrice: number
  discountPercent: number
  totalStock: number
  soldCount: number
  isActive: boolean
  isAmazingOffer: boolean
  createdAt: string
}

export interface AdminImage {
  id: number
  url: string
  displayOrder: number
}

export interface AdminVariant {
  id?: number
  colorName?: string | null
  colorHex?: string | null
  sku?: string | null
  price: number
  discountPercent: number
  stock: number
}

export interface AdminProduct {
  id: number
  title: string
  englishTitle?: string | null
  slug: string
  description: string
  categoryId: number
  brandId?: number | null
  isActive: boolean
  isAmazingOffer: boolean
  offerEndsAt?: string | null
  images: AdminImage[]
  variants: AdminVariant[]
  specifications: Specification[]
}

export interface ProductInput {
  title: string
  englishTitle?: string | null
  slug?: string | null
  description: string
  categoryId: number
  brandId?: number | null
  isActive: boolean
  isAmazingOffer: boolean
  offerEndsAt?: string | null
  variants: AdminVariant[]
  specifications: Specification[]
}

export interface AdminCategory {
  id: number
  name: string
  slug: string
  icon?: string | null
  imageUrl?: string | null
  parentId?: number | null
  parentName?: string | null
  displayOrder: number
  isActive: boolean
  productCount: number
}

export interface AdminBrand {
  id: number
  name: string
  englishName: string
  slug: string
  logoUrl?: string | null
  productCount: number
}

export interface AdminOrderRow {
  id: number
  orderNumber: string
  customerName: string
  customerPhone: string
  status: OrderStatus
  payableAmount: number
  itemsCount: number
  shippingMethod: ShippingMethod
  createdAt: string
  paidAt?: string | null
}

export interface AdminUser {
  id: number
  fullName: string
  phoneNumber: string
  email?: string | null
  role: UserRole
  isActive: boolean
  createdAt: string
  ordersCount: number
  totalSpent: number
}

export interface AdminReview {
  id: number
  productId: number
  productTitle: string
  productSlug: string
  userName: string
  rating: number
  title: string
  comment: string
  isApproved: boolean
  createdAt: string
}

export interface AdminBanner {
  id: number
  title: string
  imageUrl: string
  linkUrl?: string | null
  position: BannerPosition
  displayOrder: number
  isActive: boolean
}

export interface Coupon {
  id: number
  code: string
  type: CouponType
  value: number
  maxDiscountAmount?: number | null
  minOrderAmount: number
  expiresAt?: string | null
  usageLimit?: number | null
  usedCount: number
  isActive: boolean
  createdAt: string
}
