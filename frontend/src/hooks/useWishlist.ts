import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import toast from 'react-hot-toast'
import { useNavigate } from 'react-router'
import { del, errorMessage, get, post } from '@/lib/api'
import type { ProductCard } from '@/lib/types'
import { useAuthStore } from '@/stores/auth'

export function useWishlist() {
  const token = useAuthStore((s) => s.token)
  const qc = useQueryClient()
  const navigate = useNavigate()

  const query = useQuery({
    queryKey: ['wishlist'],
    queryFn: () => get<ProductCard[]>('/wishlist'),
    enabled: !!token,
  })
  const ids = new Set((query.data ?? []).map((p) => p.id))

  const toggle = useMutation({
    mutationFn: async (productId: number) => {
      if (ids.has(productId)) {
        await del(`/wishlist/${productId}`)
        return false
      }
      await post(`/wishlist/${productId}`)
      return true
    },
    onSuccess: (added) => {
      toast.success(added ? 'به علاقه‌مندی‌ها اضافه شد' : 'از علاقه‌مندی‌ها حذف شد')
      qc.invalidateQueries({ queryKey: ['wishlist'] })
      qc.invalidateQueries({ queryKey: ['product'] })
    },
    onError: (e) => toast.error(errorMessage(e)),
  })

  return {
    items: query.data ?? [],
    isLoading: query.isLoading,
    has: (id: number) => ids.has(id),
    toggle: (id: number) => {
      if (!token) {
        toast('برای افزودن به علاقه‌مندی‌ها وارد حساب خود شوید')
        navigate('/login')
        return
      }
      toggle.mutate(id)
    },
  }
}
