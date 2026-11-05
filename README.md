# 🛍️ بازارچه — فروشگاه اینترنتی فول‌استک

یک فروشگاه اینترنتی کامل به سبک دیجی‌کالا، ساخته‌شده به عنوان نمونه‌کار.

| بخش | تکنولوژی |
|---|---|
| Backend | .NET 10 Web API · Clean Architecture · EF Core 10 · PostgreSQL 17 · JWT · Rate Limiting · OpenAPI + Scalar |
| Frontend | React 19 · TypeScript · Vite · Tailwind CSS v4 · React Router · TanStack Query · Zustand · Swiper · Recharts |
| DevOps | Docker multi-stage builds · Docker Compose · Nginx (SPA + reverse proxy) |

## اجرا با Docker (پیشنهادی)

```bash
cp .env.example .env        # اختیاری
docker compose up -d --build
```

- فروشگاه: http://localhost:3000
- مستندات API (Scalar): http://localhost:5080/scalar/v1

در اولین اجرا دیتابیس ساخته می‌شود و داده‌های نمایشی (۶۰ محصول، دسته‌بندی‌ها، برندها، بنرها، کاربران، نظرات و ۴۵ سفارش) به‌صورت خودکار اضافه می‌شوند. تصاویر نمایشی SVG هستند و توسط خود برنامه تولید می‌شوند.

| نقش | موبایل | رمز |
|---|---|---|
| مدیر | `09120000000` | `Admin@123` |
| مشتری | `09121111111` | `Demo@123` |

کدهای تخفیف تستی: `WELCOME` (۱۰٪ تا سقف ۵۰۰ هزار تومان)، `BAZAAR50` (۵۰ هزار تومان)، `EXPIRED` (منقضی).

برای پاک کردن کامل داده‌ها: `docker compose down -v`

## اجرای محلی برای توسعه (Visual Studio 2026 / VS Code)

1. یک PostgreSQL روی `localhost:5432` (user/pass: `postgres/postgres`) — یا فقط `docker compose up -d db` و connection string را در `appsettings.json` تنظیم کنید.
2. Backend: `backend/Bazaarche.slnx` را باز کنید و پروژه `Bazaarche.Api` را اجرا کنید (http://localhost:5080).
3. Frontend:
   ```bash
   cd frontend && npm install && npm run dev   # http://localhost:5173 (درخواست‌های /api به 5080 پراکسی می‌شوند)
   ```

### Migrations
در حالت پیش‌فرض اگر هیچ migration‌ای وجود نداشته باشد، دیتابیس با `EnsureCreated` ساخته می‌شود. برای سوئیچ به migrations:
```bash
cd backend
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate -p src/Bazaarche.Infrastructure -s src/Bazaarche.Api -o Persistence/Migrations
```
از این به بعد برنامه در استارت `Migrate()` می‌کند. (اگر دیتابیس قبلاً با EnsureCreated ساخته شده، یک بار `docker compose down -v` بزنید.)

## امکانات

**فروشگاه:** صفحه اصلی با اسلایدر، دسته‌بندی‌ها، پیشنهاد شگفت‌انگیز با شمارش معکوس، پرفروش‌ترین/جدیدترین/بیشترین تخفیف/محبوب‌ترین، برندها ·
مگامنو · جستجو با پیشنهاد زنده · لیست محصولات با فیلتر (برند، قیمت، موجود، تخفیف‌دار، شگفت‌انگیز)، مرتب‌سازی و صفحه‌بندی ·
صفحه محصول با گالری، انتخاب رنگ (تنوع با قیمت/موجودی جدا)، مشخصات فنی، نظرات و امتیازدهی (نشان «خریدار»)، کالاهای مشابه ·
سبد خرید مهمان (localStorage) و ادغام خودکار با سبد سرور بعد از ورود · تسویه حساب با آدرس، شیوه ارسال (رایگان بالای ۱.۵ میلیون)، کد تخفیف ·
درگاه پرداخت شبیه‌سازی‌شده (موفق/ناموفق، پرداخت مجدد) · رزرو موجودی هنگام ثبت سفارش و برگشت آن در صورت لغو ·
پنل کاربری: سفارش‌ها با وضعیت و مراحل، آدرس‌ها، علاقه‌مندی‌ها، ویرایش پروفایل و رمز · طراحی ریسپانسیو با نوار پایین موبایل.

**پنل مدیریت:** داشبورد (درآمد، نمودار فروش ۳۰ روزه، وضعیت سفارش‌ها، موجودی کم، پرفروش‌ها) ·
مدیریت محصولات (تنوع‌ها، مشخصات، آپلود و مرتب‌سازی تصاویر، شگفت‌انگیز با زمان پایان) · دسته‌بندی درختی با آیکن · برندها با لوگو ·
سفارش‌ها (جستجو، فیلتر، تغییر وضعیت با قوانین مجاز، کد رهگیری) · کاربران (نقش و مسدودسازی) · تایید نظرات · بنرها · کدهای تخفیف.

## ساختار

```
backend/
  src/Bazaarche.Domain          entities, enums (بدون وابستگی)
  src/Bazaarche.Application     use-case services, DTOs, validation, interfaces
  src/Bazaarche.Infrastructure  EF Core + PostgreSQL, JWT, PBKDF2, file storage, seeding
  src/Bazaarche.Api             controllers, middleware, Program.cs
frontend/
  src/components  layout, ui, product
  src/pages       store pages, profile/, admin/
  src/hooks       useCart, useWishlist
  src/stores      auth & guest cart (zustand)
```

> این پروژه صرفاً نمایشی است؛ نام‌ها، قیمت‌ها و درگاه پرداخت واقعی نیستند.
