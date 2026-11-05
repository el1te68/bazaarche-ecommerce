import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { errorMessage, get, post, put, del } from '@/lib/api'
import type { Cart, CartLine } from '@/lib/types'
import { useAuthStore } from '@/stores/auth'
import { useGuestCart } from '@/stores/guestCart'

const emptyCart: Cart = { items: [], totalQuantity: 0, itemsTotal: 0, productsDiscount: 0, payableTotal: 0 }

/**
 * Unified cart: server cart for signed-in users, browser cart (priced by the API) for guests.
 */
export function useCart() {
  const token = useAuthStore((s) => s.token)
  const guest = useGuestCart()
  const qc = useQueryClient()
  const key = token ? ['cart', 'user'] : ['cart', 'guest', guest.lines]

  const query = useQuery({
    queryKey: key,
    queryFn: () => (token ? get<Cart>('/cart') : guest.lines.length ? post<Cart>('/cart/preview', guest.lines) : Promise.resolve(emptyCart)),
    placeholderData: keepPreviousData,
  })

  const onServerCart = (cart: Cart) => qc.setQueryData(['cart', 'user'], cart)

  const addMutation = useMutation({
    mutationFn: async ({ variantId, quantity, max }: { variantId: number; quantity: number; max: number }) => {
      if (token) return onServerCart(await post<Cart>('/cart/items', { variantId, quantity }))
      const current = guest.lines.find((l) => l.variantId === variantId)?.quantity ?? 0
      if (current + quantity > max) throw new Error(`حداکثر ${max} عدد از این کالا قابل سفارش است.`)
      guest.add(variantId, quantity)
    },
    onError: (e) => toast.error(e instanceof Error && !('isAxiosError' in e) ? e.message : errorMessage(e)),
  })

  const updateMutation = useMutation({
    mutationFn: async ({ variantId, quantity }: CartLine) => {
      if (token) {
        onServerCart(quantity <= 0 ? await del<Cart>(`/cart/items/${variantId}`) : await put<Cart>(`/cart/items/${variantId}`, { quantity }))
      } else guest.set(variantId, quantity)
    },
    onError: (e) => toast.error(errorMessage(e)),
  })

  const cart = query.data ?? emptyCart
  return {
    cart,
    isLoading: query.isLoading,
    isFetching: query.isFetching || updateMutation.isPending,
    quantityOf: (variantId: number) => cart.items.find((i) => i.variantId === variantId)?.quantity ?? 0,
    add: (variantId: number, quantity: number, max: number) => addMutation.mutateAsync({ variantId, quantity, max }),
    adding: addMutation.isPending,
    update: (variantId: number, quantity: number) => updateMutation.mutate({ variantId, quantity }),
    remove: (variantId: number) => updateMutation.mutate({ variantId, quantity: 0 }),
  }
}

/** Called right after login/register: moves the guest cart to the user's server cart. */
export async function mergeGuestCart() {
  const { lines, clear } = useGuestCart.getState()
  if (lines.length === 0) return
  try {
    await post<Cart>('/cart/merge', lines)
    clear()
  } catch {
    /* keep the guest cart if merging failed */
  }
}
