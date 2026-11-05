import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type { CartLine } from '@/lib/types'

/** Cart of a visitor who has not signed in yet. Merged into the server cart after login. */
interface GuestCartState {
  lines: CartLine[]
  set: (variantId: number, quantity: number) => void
  add: (variantId: number, quantity: number) => void
  clear: () => void
}

export const useGuestCart = create<GuestCartState>()(
  persist(
    (set) => ({
      lines: [],
      set: (variantId, quantity) =>
        set((s) => ({
          lines: quantity <= 0
            ? s.lines.filter((l) => l.variantId !== variantId)
            : s.lines.some((l) => l.variantId === variantId)
              ? s.lines.map((l) => (l.variantId === variantId ? { ...l, quantity } : l))
              : [...s.lines, { variantId, quantity }],
        })),
      add: (variantId, quantity) =>
        set((s) => {
          const existing = s.lines.find((l) => l.variantId === variantId)
          return {
            lines: existing
              ? s.lines.map((l) => (l.variantId === variantId ? { ...l, quantity: l.quantity + quantity } : l))
              : [...s.lines, { variantId, quantity }],
          }
        }),
      clear: () => set({ lines: [] }),
    }),
    { name: 'bazaarche-guest-cart' },
  ),
)
