import {
  BatteryCharging, Book, BookOpen, Briefcase, Camera, CookingPot, Dumbbell, Footprints, Headphones, Laptop, LayoutGrid,
  PenTool, Puzzle, Refrigerator, Scissors, Shirt, ShoppingBag, ShoppingBasket, Smartphone, Sparkles, Speaker, SprayCan,
  Tablet, Tv, Watch, Wind, type LucideIcon,
} from 'lucide-react'

/** Icons an admin can pick for a category (stored by name in the database). */
export const categoryIcons: Record<string, LucideIcon> = {
  smartphone: Smartphone, headphones: Headphones, 'battery-charging': BatteryCharging, laptop: Laptop, tablet: Tablet,
  watch: Watch, speaker: Speaker, camera: Camera, refrigerator: Refrigerator, tv: Tv, 'cooking-pot': CookingPot, wind: Wind,
  shirt: Shirt, 'shopping-bag': ShoppingBag, footprints: Footprints, sparkles: Sparkles, 'spray-can': SprayCan,
  scissors: Scissors, 'book-open': BookOpen, book: Book, 'pen-tool': PenTool, dumbbell: Dumbbell, briefcase: Briefcase,
  puzzle: Puzzle, 'shopping-basket': ShoppingBasket,
}

export function CategoryIcon({ name, size = 22, className }: { name?: string | null; size?: number; className?: string }) {
  const Icon = (name && categoryIcons[name]) || LayoutGrid
  return <Icon size={size} className={className} />
}
