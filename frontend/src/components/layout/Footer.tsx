import { Headphones, RotateCcw, ShieldCheck, Truck, Wallet } from 'lucide-react'
import { Link } from 'react-router'
import { Logo } from './Logo'

const features = [
  { icon: Truck, title: 'تحویل اکسپرس' },
  { icon: Headphones, title: 'پشتیبانی ۲۴ ساعته' },
  { icon: Wallet, title: 'پرداخت در محل' },
  { icon: RotateCcw, title: '۷ روز ضمانت بازگشت' },
  { icon: ShieldCheck, title: 'ضمانت اصل بودن کالا' },
]

export function Footer() {
  return (
    <footer className="mt-10 border-t border-line bg-white pb-20 md:pb-0">
      <div className="container-x py-8">
        <div className="flex flex-wrap items-center justify-between gap-4">
          <Logo />
          <p className="text-sm text-muted">تلفن پشتیبانی: ۰۲۱-۱۲۳۴۵۶۷۸ | ۷ روز هفته، ۲۴ ساعته پاسخگوی شما هستیم</p>
        </div>
        <div className="my-8 grid grid-cols-2 gap-6 sm:grid-cols-5">
          {features.map(({ icon: Icon, title }) => (
            <div key={title} className="flex flex-col items-center gap-2 text-center">
              <Icon size={40} strokeWidth={1.3} className="text-brand-500" />
              <span className="text-xs text-ink/80">{title}</span>
            </div>
          ))}
        </div>
        <div className="grid gap-8 border-t border-line pt-8 sm:grid-cols-2 lg:grid-cols-4">
          <div>
            <h4 className="mb-3 font-bold">با بازارچه</h4>
            <ul className="space-y-2 text-sm text-muted">
              <li>درباره بازارچه</li><li>فرصت‌های شغلی</li><li>تماس با ما</li>
            </ul>
          </div>
          <div>
            <h4 className="mb-3 font-bold">خدمات مشتریان</h4>
            <ul className="space-y-2 text-sm text-muted">
              <li>پاسخ به پرسش‌های متداول</li><li>رویه‌های بازگرداندن کالا</li><li>شرایط استفاده</li><li>حریم خصوصی</li>
            </ul>
          </div>
          <div>
            <h4 className="mb-3 font-bold">راهنمای خرید</h4>
            <ul className="space-y-2 text-sm text-muted">
              <li><Link to="/cart" className="hover:text-ink">سبد خرید</Link></li>
              <li><Link to="/profile/orders" className="hover:text-ink">پیگیری سفارش</Link></li>
              <li>شیوه‌های پرداخت</li>
            </ul>
          </div>
          <div>
            <h4 className="mb-3 font-bold">از تخفیف‌ها و جدیدترین‌ها باخبر شوید</h4>
            <form className="flex gap-2" onSubmit={(e) => e.preventDefault()}>
              <input className="input" placeholder="ایمیل شما" type="email" />
              <button className="btn bg-gray-300 text-white hover:bg-gray-400">ثبت</button>
            </form>
          </div>
        </div>
        <div className="mt-8 rounded-xl bg-surface p-5 text-sm leading-7 text-ink/80">
          <h4 className="mb-1 font-bold">فروشگاه اینترنتی بازارچه، بررسی، انتخاب و خرید آنلاین</h4>
          بازارچه یک پروژه نمونه‌کار فول‌استک (React + .NET + PostgreSQL) است که امکانات یک فروشگاه اینترنتی کامل را پیاده‌سازی می‌کند:
          کاتالوگ محصولات، جستجو و فیلتر، سبد خرید، پرداخت آنلاین (شبیه‌سازی‌شده)، پیگیری سفارش و پنل مدیریت.
        </div>
      </div>
      <div className="border-t border-line py-4 text-center text-xs text-muted">
        © {new Date().getFullYear()} بازارچه — پروژه نمونه‌کار. تمامی محصولات و اطلاعات نمایشی هستند.
      </div>
    </footer>
  )
}
