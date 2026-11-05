namespace Bazaarche.Infrastructure.Seeding;

/// <summary>Static demo data used by <see cref="DataSeeder"/>.</summary>
internal static class SeedCatalog
{
    public record CategorySeed(string Name, string Slug, string Icon, CategorySeed[]? Children = null);

    public record BrandSeed(string Name, string EnglishName, string Slug, string Color);

    public record ColorSeed(string Name, string Hex);

    public record ProductSeed(
        string Title,
        string EnglishTitle,
        string Category,
        string? Brand,
        string Kind,
        long Price,
        int Discount,
        int Stock,
        ColorSeed[] Colors,
        string Specs,
        bool Amazing = false,
        string? Label = null);

    public static readonly CategorySeed[] Categories =
    [
        new("موبایل", "mobile", "smartphone",
        [
            new("گوشی موبایل", "mobile-phone", "smartphone"),
            new("هدفون و هندزفری", "headphones", "headphones"),
            new("پاوربانک و شارژر", "power-bank", "battery-charging")
        ]),
        new("کالای دیجیتال", "digital", "laptop",
        [
            new("لپ‌تاپ", "laptop", "laptop"),
            new("تبلت", "tablet", "tablet"),
            new("ساعت و مچ‌بند هوشمند", "smartwatch", "watch"),
            new("اسپیکر", "speaker", "speaker"),
            new("دوربین", "camera", "camera")
        ]),
        new("لوازم خانگی", "home-appliances", "refrigerator",
        [
            new("تلویزیون", "tv", "tv"),
            new("لوازم آشپزخانه", "kitchen", "cooking-pot"),
            new("جاروبرقی", "vacuum", "wind")
        ]),
        new("مد و پوشاک", "fashion", "shirt",
        [
            new("پوشاک مردانه", "men-clothing", "shirt"),
            new("پوشاک زنانه", "women-clothing", "shopping-bag"),
            new("کفش", "shoes", "footprints")
        ]),
        new("زیبایی و سلامت", "beauty", "sparkles",
        [
            new("عطر و ادکلن", "perfume", "spray-can"),
            new("اصلاح و پیرایش", "shaver", "scissors")
        ]),
        new("کتاب و لوازم تحریر", "books", "book-open",
        [
            new("کتاب", "book", "book"),
            new("لوازم تحریر", "stationery", "pen-tool")
        ]),
        new("ورزش و سفر", "sports", "dumbbell",
        [
            new("لوازم ورزشی", "sports-gear", "dumbbell"),
            new("کوله و چمدان", "bags", "briefcase")
        ]),
        new("اسباب‌بازی", "toys", "puzzle"),
        new("سوپرمارکت", "supermarket", "shopping-basket")
    ];

    public static readonly BrandSeed[] Brands =
    [
        new("سامسونگ", "Samsung", "samsung", "#1428a0"),
        new("اپل", "Apple", "apple", "#111111"),
        new("شیائومی", "Xiaomi", "xiaomi", "#ff6900"),
        new("هوآوی", "Huawei", "huawei", "#cf0a2c"),
        new("لنوو", "Lenovo", "lenovo", "#e2231a"),
        new("ایسوس", "ASUS", "asus", "#00539b"),
        new("سونی", "Sony", "sony", "#000000"),
        new("ال‌جی", "LG", "lg", "#a50034"),
        new("انکر", "Anker", "anker", "#00a7e1"),
        new("جی‌بی‌ال", "JBL", "jbl", "#ff3300"),
        new("کانن", "Canon", "canon", "#cc0000"),
        new("فیلیپس", "Philips", "philips", "#0b5ed7"),
        new("بوش", "Bosch", "bosch", "#e20015"),
        new("تفال", "Tefal", "tefal", "#d4001a"),
        new("دلونگی", "DeLonghi", "delonghi", "#1d3c78"),
        new("نایکی", "Nike", "nike", "#111111"),
        new("آدیداس", "Adidas", "adidas", "#000000"),
        new("دیور", "Dior", "dior", "#222222"),
        new("شنل", "Chanel", "chanel", "#000000"),
        new("براون", "Braun", "braun", "#333333"),
        new("سامسونایت", "Samsonite", "samsonite", "#003a70"),
        new("لگو", "LEGO", "lego", "#d01012"),
        new("لاوازا", "Lavazza", "lavazza", "#1b2a5c")
    ];

    private static ColorSeed C(string name, string hex) => new(name, hex);

    private static readonly ColorSeed[] None = [];
    private static readonly ColorSeed Black = C("مشکی", "#1f2937");
    private static readonly ColorSeed White = C("سفید", "#f1f5f9");
    private static readonly ColorSeed Silver = C("نقره‌ای", "#cbd5e1");
    private static readonly ColorSeed Blue = C("آبی", "#2563eb");
    private static readonly ColorSeed Navy = C("سرمه‌ای", "#1e3a8a");
    private static readonly ColorSeed Gray = C("طوسی", "#6b7280");
    private static readonly ColorSeed Green = C("سبز", "#16a34a");
    private static readonly ColorSeed Purple = C("بنفش", "#7c3aed");
    private static readonly ColorSeed Pink = C("صورتی", "#ec4899");
    private static readonly ColorSeed Gold = C("طلایی", "#c8a97e");
    private static readonly ColorSeed Red = C("قرمز", "#dc2626");
    private static readonly ColorSeed Beige = C("کرم", "#d6c4a8");
    private static readonly ColorSeed Olive = C("زیتونی", "#65733b");
    private static readonly ColorSeed Orange = C("نارنجی", "#f97316");

    public static readonly ProductSeed[] Products =
    [
        // ---------- Mobile phones ----------
        new("گوشی موبایل سامسونگ مدل Galaxy S25 Ultra دو سیم کارت ظرفیت ۲۵۶ گیگابایت و رم ۱۲ گیگابایت", "Samsung Galaxy S25 Ultra 256GB RAM 12GB",
            "mobile-phone", "samsung", "phone", 98_500_000, 7, 12, [C("تیتانیوم مشکی", "#27272a"), C("تیتانیوم نقره‌ای", "#a1a1aa"), C("تیتانیوم آبی", "#1e3a8a")],
            "اندازه صفحه نمایش:۶.۹ اینچ|فناوری صفحه نمایش:Dynamic AMOLED 2X|تراشه:Snapdragon 8 Elite for Galaxy|حافظه داخلی:۲۵۶ گیگابایت|مقدار RAM:۱۲ گیگابایت|دوربین اصلی:۲۰۰ مگاپیکسل|ظرفیت باتری:۵۰۰۰ میلی‌آمپر ساعت|نسخه سیستم عامل:Android 15 - One UI 7|پشتیبانی از شبکه:5G", true),
        new("گوشی موبایل اپل مدل iPhone 16 Pro Max ظرفیت ۲۵۶ گیگابایت و رم ۸ گیگابایت", "Apple iPhone 16 Pro Max 256GB",
            "mobile-phone", "apple", "phone", 145_000_000, 0, 6, [C("طلایی صحرایی", "#c8a97e"), C("تیتانیوم طبیعی", "#a8a29e"), C("مشکی", "#111827")],
            "اندازه صفحه نمایش:۶.۹ اینچ|فناوری صفحه نمایش:Super Retina XDR OLED|تراشه:Apple A18 Pro|حافظه داخلی:۲۵۶ گیگابایت|دوربین اصلی:۴۸ مگاپیکسل|نسخه سیستم عامل:iOS 18|پشتیبانی از شبکه:5G|مقاومت در برابر آب:IP68"),
        new("گوشی موبایل شیائومی مدل Redmi Note 14 Pro 4G ظرفیت ۲۵۶ گیگابایت و رم ۸ گیگابایت", "Xiaomi Redmi Note 14 Pro 4G 256GB",
            "mobile-phone", "xiaomi", "phone", 24_900_000, 12, 40, [C("بنفش", "#7c3aed"), Black, C("سبز", "#15803d")],
            "اندازه صفحه نمایش:۶.۶۷ اینچ|فناوری صفحه نمایش:AMOLED 120Hz|تراشه:Helio G100 Ultra|حافظه داخلی:۲۵۶ گیگابایت|مقدار RAM:۸ گیگابایت|دوربین اصلی:۲۰۰ مگاپیکسل|ظرفیت باتری:۵۵۰۰ میلی‌آمپر ساعت", true),
        new("گوشی موبایل سامسونگ مدل Galaxy A56 5G ظرفیت ۲۵۶ گیگابایت و رم ۸ گیگابایت", "Samsung Galaxy A56 5G 256GB",
            "mobile-phone", "samsung", "phone", 38_700_000, 5, 25, [C("خاکستری", "#4b5563"), C("صورتی", "#f9a8d4"), C("سبز روشن", "#86efac")],
            "اندازه صفحه نمایش:۶.۷ اینچ|فناوری صفحه نمایش:Super AMOLED|تراشه:Exynos 1580|حافظه داخلی:۲۵۶ گیگابایت|مقدار RAM:۸ گیگابایت|ظرفیت باتری:۵۰۰۰ میلی‌آمپر ساعت|پشتیبانی از شبکه:5G"),
        new("گوشی موبایل شیائومی مدل Poco X7 Pro 5G ظرفیت ۵۱۲ گیگابایت و رم ۱۲ گیگابایت", "Xiaomi Poco X7 Pro 5G 512GB",
            "mobile-phone", "xiaomi", "phone", 29_500_000, 9, 18, [Black, C("زرد", "#facc15")],
            "اندازه صفحه نمایش:۶.۶۷ اینچ|تراشه:Dimensity 8400 Ultra|حافظه داخلی:۵۱۲ گیگابایت|مقدار RAM:۱۲ گیگابایت|ظرفیت باتری:۶۰۰۰ میلی‌آمپر ساعت|شارژ سریع:۹۰ وات"),
        new("گوشی موبایل هوآوی مدل Nova 13 ظرفیت ۲۵۶ گیگابایت و رم ۱۲ گیگابایت", "Huawei Nova 13 256GB",
            "mobile-phone", "huawei", "phone", 31_200_000, 0, 0, [Black, Purple],
            "اندازه صفحه نمایش:۶.۷ اینچ|فناوری صفحه نمایش:OLED|حافظه داخلی:۲۵۶ گیگابایت|مقدار RAM:۱۲ گیگابایت|ظرفیت باتری:۵۰۰۰ میلی‌آمپر ساعت"),
        new("گوشی موبایل اپل مدل iPhone 15 ظرفیت ۱۲۸ گیگابایت و رم ۶ گیگابایت", "Apple iPhone 15 128GB",
            "mobile-phone", "apple", "phone", 89_000_000, 4, 8, [C("مشکی", "#1f2937"), C("آبی", "#bfdbfe"), C("صورتی", "#fbcfe8")],
            "اندازه صفحه نمایش:۶.۱ اینچ|تراشه:Apple A16 Bionic|حافظه داخلی:۱۲۸ گیگابایت|دوربین اصلی:۴۸ مگاپیکسل|درگاه شارژ:USB Type-C"),

        // ---------- Headphones ----------
        new("هدفون بی سیم سونی مدل WH-1000XM5 با قابلیت حذف نویز", "Sony WH-1000XM5 Wireless Headphones",
            "headphones", "sony", "headphones", 28_900_000, 15, 10, [Black, Silver],
            "نوع اتصال:بی‌سیم (بلوتوث ۵.۲)|حذف نویز فعال:دارد|عمر باتری:تا ۳۰ ساعت|وزن:۲۵۰ گرم|میکروفون:دارد", true),
        new("هندزفری بلوتوثی اپل مدل AirPods Pro 2 با کیس USB-C", "Apple AirPods Pro 2 USB-C",
            "headphones", "apple", "earbuds", 19_500_000, 6, 20, [White],
            "نوع اتصال:بلوتوث ۵.۳|حذف نویز فعال:دارد|عمر باتری:۶ ساعت (تا ۳۰ ساعت با کیس)|مقاومت در برابر آب:IP54"),
        new("هندزفری بلوتوثی شیائومی مدل Redmi Buds 6 Pro", "Xiaomi Redmi Buds 6 Pro",
            "headphones", "xiaomi", "earbuds", 3_450_000, 22, 60, [Black, White, Purple],
            "نوع اتصال:بلوتوث ۵.۳|حذف نویز فعال:تا ۵۵ دسی‌بل|عمر باتری:تا ۳۶ ساعت با کیس|کدک:LDAC", true),
        new("هدفون گیمینگ ایسوس مدل ROG Delta S", "ASUS ROG Delta S Gaming Headset",
            "headphones", "asus", "headphones", 11_800_000, 0, 7, [Black],
            "نوع اتصال:USB-C|نورپردازی:RGB|میکروفون:قابل جدا شدن با حذف نویز|پاسخ فرکانسی:۲۰ تا ۴۰۰۰۰ هرتز"),
        new("هندزفری بلوتوثی سامسونگ مدل Galaxy Buds3 Pro", "Samsung Galaxy Buds3 Pro",
            "headphones", "samsung", "earbuds", 14_200_000, 10, 15, [Silver, White],
            "نوع اتصال:بلوتوث ۵.۴|حذف نویز فعال:دارد|عمر باتری:تا ۲۶ ساعت با کیس|مقاومت در برابر آب:IP57"),

        // ---------- Power banks & chargers ----------
        new("پاوربانک انکر مدل PowerCore A1384 ظرفیت ۲۰۰۰۰ میلی‌آمپر ساعت", "Anker PowerCore 20000mAh",
            "power-bank", "anker", "powerbank", 3_900_000, 8, 35, [Black, Blue],
            "ظرفیت:۲۰۰۰۰ میلی‌آمپر ساعت|توان خروجی:۲۲.۵ وات|تعداد درگاه خروجی:۳|نمایشگر:دارد", Label: "20000"),
        new("شارژر دیواری انکر مدل Nano II توان ۶۵ وات GaN", "Anker Nano II 65W GaN Charger",
            "power-bank", "anker", "charger", 2_650_000, 0, 50, [White],
            "توان خروجی:۶۵ وات|فناوری:GaN II|نوع درگاه:USB-C|پشتیبانی از شارژ لپ‌تاپ:دارد", Label: "65W"),
        new("پاوربانک مگنتی شیائومی مدل P05ZM ظرفیت ۱۰۰۰۰ میلی‌آمپر ساعت", "Xiaomi Magnetic Power Bank 10000mAh",
            "power-bank", "xiaomi", "powerbank", 1_980_000, 18, 3, [Silver],
            "ظرفیت:۱۰۰۰۰ میلی‌آمپر ساعت|شارژ بی‌سیم مغناطیسی:دارد|توان خروجی:۲۲.۵ وات", true, "10000"),

        // ---------- Laptops ----------
        new("لپ تاپ ۱۶ اینچی ایسوس مدل Vivobook 16 X1605VA-i7 13620H 16GB 512SSD", "ASUS Vivobook 16 X1605VA",
            "laptop", "asus", "laptop", 62_000_000, 6, 9, [Silver, C("آبی تیره", "#1e3a8a")],
            "پردازنده:Intel Core i7-13620H|حافظه RAM:۱۶ گیگابایت|حافظه داخلی:۵۱۲ گیگابایت SSD|اندازه صفحه نمایش:۱۶ اینچ|پردازنده گرافیکی:Intel UHD|سیستم عامل:Windows 11"),
        new("لپ تاپ ۱۳ اینچی اپل مدل MacBook Air M4 16GB 256SSD", "Apple MacBook Air 13 M4",
            "laptop", "apple", "laptop", 118_000_000, 3, 5, [C("نیمه‌شب", "#1e293b"), Silver, C("آبی آسمانی", "#93c5fd")],
            "پردازنده:Apple M4 (۱۰ هسته‌ای)|حافظه RAM:۱۶ گیگابایت|حافظه داخلی:۲۵۶ گیگابایت SSD|اندازه صفحه نمایش:۱۳.۶ اینچ Liquid Retina|عمر باتری:تا ۱۸ ساعت|سیستم عامل:macOS"),
        new("لپ تاپ ۱۵.۶ اینچی لنوو مدل IdeaPad Slim 3 15IRH8-i5 16GB 512SSD", "Lenovo IdeaPad Slim 3 15IRH8",
            "laptop", "lenovo", "laptop", 41_500_000, 11, 14, [Gray],
            "پردازنده:Intel Core i5-13420H|حافظه RAM:۱۶ گیگابایت|حافظه داخلی:۵۱۲ گیگابایت SSD|اندازه صفحه نمایش:۱۵.۶ اینچ FHD|سیستم عامل:Free DOS", true),
        new("لپ تاپ گیمینگ ۱۶ اینچی ایسوس مدل ROG Strix G16 G614JIR-i9 32GB 1TB RTX4070", "ASUS ROG Strix G16 RTX 4070",
            "laptop", "asus", "laptop", 145_000_000, 0, 4, [C("خاکستری تیره", "#374151")],
            "پردازنده:Intel Core i9-14900HX|حافظه RAM:۳۲ گیگابایت DDR5|حافظه داخلی:۱ ترابایت SSD|پردازنده گرافیکی:NVIDIA RTX 4070 8GB|نرخ نوسازی تصویر:۲۴۰ هرتز"),
        new("لپ تاپ ۱۴ اینچی لنوو مدل ThinkPad E14 Gen 5-i7 16GB 1TB", "Lenovo ThinkPad E14 Gen 5",
            "laptop", "lenovo", "laptop", 72_300_000, 5, 6, [Black],
            "پردازنده:Intel Core i7-1355U|حافظه RAM:۱۶ گیگابایت|حافظه داخلی:۱ ترابایت SSD|اندازه صفحه نمایش:۱۴ اینچ WUXGA|کیبورد:ضد آب با نور پس‌زمینه"),

        // ---------- Tablets ----------
        new("تبلت سامسونگ مدل Galaxy Tab S10 FE ظرفیت ۱۲۸ گیگابایت و رم ۸ گیگابایت", "Samsung Galaxy Tab S10 FE",
            "tablet", "samsung", "tablet", 34_800_000, 8, 11, [Gray, Silver],
            "اندازه صفحه نمایش:۱۰.۹ اینچ|حافظه داخلی:۱۲۸ گیگابایت|مقدار RAM:۸ گیگابایت|قلم S Pen:همراه دستگاه|ظرفیت باتری:۸۰۰۰ میلی‌آمپر ساعت"),
        new("تبلت اپل مدل iPad Air 11 اینچ M3 ظرفیت ۱۲۸ گیگابایت", "Apple iPad Air 11 M3 128GB",
            "tablet", "apple", "tablet", 69_000_000, 0, 7, [C("خاکستری فضایی", "#4b5563"), C("آبی", "#93c5fd"), C("بنفش", "#c4b5fd")],
            "اندازه صفحه نمایش:۱۱ اینچ Liquid Retina|تراشه:Apple M3|حافظه داخلی:۱۲۸ گیگابایت|پشتیبانی از Apple Pencil Pro:دارد"),
        new("تبلت شیائومی مدل Pad 7 ظرفیت ۲۵۶ گیگابایت و رم ۸ گیگابایت", "Xiaomi Pad 7 256GB",
            "tablet", "xiaomi", "tablet", 22_400_000, 14, 20, [Gray, Blue],
            "اندازه صفحه نمایش:۱۱.۲ اینچ 3.2K|تراشه:Snapdragon 7+ Gen 3|حافظه داخلی:۲۵۶ گیگابایت|ظرفیت باتری:۸۸۵۰ میلی‌آمپر ساعت", true),

        // ---------- Smartwatches ----------
        new("ساعت هوشمند سامسونگ مدل Galaxy Watch8 44mm", "Samsung Galaxy Watch8 44mm",
            "smartwatch", "samsung", "watch", 17_900_000, 10, 13, [Silver, Black],
            "اندازه صفحه نمایش:۱.۵ اینچ|سنسورها:ضربان قلب، اکسیژن خون، ECG|مقاومت در برابر آب:5ATM|عمر باتری:تا ۴۰ ساعت"),
        new("ساعت هوشمند اپل مدل Watch Series 10 Aluminum 46mm", "Apple Watch Series 10 46mm",
            "smartwatch", "apple", "watch", 38_500_000, 0, 9, [C("مشکی جت", "#0f172a"), C("رزگلد", "#e8b4a0"), Silver],
            "اندازه بدنه:۴۶ میلی‌متر|جنس بدنه:آلومینیوم|سنسورها:ECG، اکسیژن خون، دمای بدن|مقاومت در برابر آب:WR50"),
        new("مچ بند هوشمند شیائومی مدل Smart Band 9", "Xiaomi Smart Band 9",
            "smartwatch", "xiaomi", "band", 2_350_000, 25, 80, [Black, Pink, Blue],
            "اندازه صفحه نمایش:۱.۶۲ اینچ AMOLED|عمر باتری:تا ۲۱ روز|مقاومت در برابر آب:5ATM|تعداد حالت‌های ورزشی:۱۵۰+", true),

        // ---------- Speakers ----------
        new("اسپیکر بلوتوثی قابل حمل جی بی ال مدل Flip 6", "JBL Flip 6 Portable Speaker",
            "speaker", "jbl", "speaker", 7_850_000, 12, 22, [Blue, Black, Red],
            "توان خروجی:۳۰ وات|مقاومت در برابر آب:IP67|عمر باتری:تا ۱۲ ساعت|نسخه بلوتوث:۵.۱", Label: "JBL"),
        new("اسپیکر بلوتوثی قابل حمل جی بی ال مدل Charge 5", "JBL Charge 5 Portable Speaker",
            "speaker", "jbl", "speaker", 11_200_000, 0, 2, [Black, Green],
            "توان خروجی:۴۰ وات|مقاومت در برابر آب:IP67|عمر باتری:تا ۲۰ ساعت|قابلیت پاوربانک:دارد", Label: "JBL"),

        // ---------- Cameras ----------
        new("دوربین دیجیتال بدون آینه کانن مدل EOS R50 به همراه لنز 18-45 میلی‌متر", "Canon EOS R50 Kit 18-45mm",
            "camera", "canon", "camera", 68_000_000, 4, 3, [Black],
            "نوع دوربین:بدون آینه|سنسور:APS-C ۲۴.۲ مگاپیکسل|فیلمبرداری:4K 30fps|لنز همراه:RF-S 18-45mm", Label: "EOS R50"),
        new("دوربین دیجیتال بدون آینه سونی مدل ZV-E10 II", "Sony ZV-E10 II",
            "camera", "sony", "camera", 59_500_000, 0, 5, [Black, White],
            "نوع دوربین:بدون آینه مخصوص ولاگ|سنسور:APS-C ۲۶ مگاپیکسل|فیلمبرداری:4K 60fps", Label: "ZV-E10"),

        // ---------- TV ----------
        new("تلویزیون OLED ال جی مدل OLED55C4 سایز ۵۵ اینچ", "LG OLED55C4 55 inch",
            "tv", "lg", "tv", 125_000_000, 9, 4, None,
            "اندازه صفحه نمایش:۵۵ اینچ|نوع پنل:OLED evo|رزولوشن:4K UHD|نرخ نوسازی:۱۴۴ هرتز|سیستم عامل:webOS", true),
        new("تلویزیون هوشمند سامسونگ مدل 65DU8000 سایز ۶۵ اینچ", "Samsung Crystal UHD 65DU8000",
            "tv", "samsung", "tv", 78_000_000, 6, 6, None,
            "اندازه صفحه نمایش:۶۵ اینچ|نوع پنل:Crystal UHD|رزولوشن:4K|سیستم عامل:Tizen"),

        // ---------- Kitchen ----------
        new("سرخ کن بدون روغن فیلیپس مدل HD9285 Airfryer XXL", "Philips Airfryer XXL HD9285",
            "kitchen", "philips", "airfryer", 18_500_000, 13, 12, [Black],
            "ظرفیت:۷.۲ لیتر|توان مصرفی:۲۰۰۰ وات|تعداد برنامه پخت:۵|قابلیت اتصال به اپلیکیشن:دارد", true),
        new("اسپرسوساز دلونگی مدل Dedica EC685", "DeLonghi Dedica EC685",
            "kitchen", "delonghi", "coffee-maker", 32_000_000, 0, 5, [Silver, Red, Black],
            "فشار پمپ:۱۵ بار|ظرفیت مخزن آب:۱.۱ لیتر|کف‌ساز شیر:دارد|توان مصرفی:۱۳۵۰ وات"),
        new("کتری برقی بوش مدل TWK3P420", "Bosch TWK3P420 Electric Kettle",
            "kitchen", "bosch", "kettle", 3_700_000, 9, 25, [White, Black],
            "ظرفیت:۱.۷ لیتر|توان مصرفی:۲۴۰۰ وات|جنس بدنه:استیل ضد زنگ|خاموش شدن خودکار:دارد"),
        new("مخلوط کن تفال مدل BL4258", "Tefal Blendforce BL4258",
            "kitchen", "tefal", "blender", 5_400_000, 0, 15, [Red, White],
            "ظرفیت پارچ:۱.۵ لیتر|توان مصرفی:۸۰۰ وات|تعداد سرعت:۲ + پالس|جنس تیغه:استیل"),

        // ---------- Vacuum ----------
        new("جارو رباتیک شیائومی مدل Robot Vacuum S20 Plus", "Xiaomi Robot Vacuum S20+",
            "vacuum", "xiaomi", "robot", 21_600_000, 16, 8, [White, Black],
            "قدرت مکش:۶۰۰۰ پاسکال|قابلیت طی کشیدن:دارد|ناوبری:LDS لیزری|کنترل با اپلیکیشن:دارد", true),
        new("جاروبرقی بوش مدل BGL6PRO1 سری ۶", "Bosch Series 6 BGL6PRO1",
            "vacuum", "bosch", "vacuum", 24_900_000, 5, 6, [Navy],
            "توان مصرفی:۸۵۰ وات|نوع کیسه:کیسه‌دار|ظرفیت کیسه:۵ لیتر|فیلتر:HEPA"),

        // ---------- Men clothing ----------
        new("تی شرت ورزشی مردانه نایکی مدل Dri-FIT Legend", "Nike Dri-FIT Legend T-Shirt",
            "men-clothing", "nike", "tshirt", 2_450_000, 20, 40, [Black, White, Navy],
            "جنس:پلی‌استر|فناوری:Dri-FIT جذب رطوبت|یقه:گرد|مناسب برای:ورزش و استفاده روزمره"),
        new("هودی مردانه آدیداس مدل Essentials 3-Stripes", "Adidas Essentials 3-Stripes Hoodie",
            "men-clothing", "adidas", "hoodie", 4_900_000, 0, 18, [Gray, Black],
            "جنس:پنبه و پلی‌استر|کلاه:دارد|جیب:کانگورویی|فصل مناسب:پاییز و زمستان"),

        // ---------- Women clothing ----------
        new("مانتو زنانه کتان مدل بهار بلند با کمربند", "Women's Linen Long Coat",
            "women-clothing", null, "coat", 2_890_000, 15, 25, [Beige, Black, Olive],
            "جنس:کتان|قد:بلند|آستین:بلند|نوع بسته شدن:دکمه", true),
        new("شومیز زنانه ساتن مدل نیلوفر", "Women's Satin Blouse",
            "women-clothing", null, "shirt", 1_650_000, 0, 30, [White, Pink, C("آبی آسمانی", "#7dd3fc")],
            "جنس:ساتن|یقه:انگلیسی|آستین:بلند|مناسب برای:مجلسی و رسمی"),

        // ---------- Shoes ----------
        new("کفش دویدن مردانه نایکی مدل Pegasus 41", "Nike Pegasus 41 Running Shoes",
            "shoes", "nike", "shoe", 8_900_000, 10, 14, [Black, Blue, Orange],
            "نوع کفش:رانینگ|جنس رویه:مش تنفسی|فناوری زیره:ReactX + Air Zoom|کاربرد:دویدن روزانه"),
        new("کفش پیاده روی آدیداس مدل Ultraboost Light", "Adidas Ultraboost Light",
            "shoes", "adidas", "shoe", 11_500_000, 18, 9, [White, Black],
            "نوع کفش:پیاده‌روی و دویدن|فناوری زیره:Light BOOST|جنس رویه:Primeknit", true),

        // ---------- Perfume ----------
        new("ادو پرفیوم مردانه دیور مدل Sauvage حجم ۱۰۰ میلی لیتر", "Dior Sauvage EDP 100ml",
            "perfume", "dior", "perfume", 21_000_000, 0, 10, None,
            "نوع:ادو پرفیوم|حجم:۱۰۰ میلی‌لیتر|گروه بویایی:تند و چوبی|مناسب برای:آقایان", Label: "SAUVAGE"),
        new("ادو پرفیوم زنانه شنل مدل Coco Mademoiselle حجم ۱۰۰ میلی لیتر", "Chanel Coco Mademoiselle EDP 100ml",
            "perfume", "chanel", "perfume", 26_500_000, 5, 6, None,
            "نوع:ادو پرفیوم|حجم:۱۰۰ میلی‌لیتر|گروه بویایی:گلی و مرکباتی|مناسب برای:بانوان", Label: "COCO"),

        // ---------- Shavers ----------
        new("ماشین اصلاح موی صورت فیلیپس مدل OneBlade Pro QP6530", "Philips OneBlade Pro QP6530",
            "shaver", "philips", "shaver", 4_750_000, 11, 30, [C("سبز لیمویی", "#84cc16")],
            "نوع:شارژی|عمر باتری:۹۰ دقیقه|ضد آب:دارد|تعداد شانه:۱۴ تنظیم طول"),
        new("ریش تراش براون مدل Series 9 Pro+", "Braun Series 9 Pro+",
            "shaver", "braun", "shaver", 22_000_000, 0, 4, [C("نقره‌ای", "#9ca3af")],
            "نوع:فویلی شارژی|عمر باتری:۶۰ دقیقه|ایستگاه شستشو:دارد|استفاده خیس و خشک:دارد"),

        // ---------- Books ----------
        new("کتاب عادت های اتمی اثر جیمز کلیر", "Atomic Habits",
            "book", null, "book", 385_000, 10, 100, None,
            "نویسنده:جیمز کلیر|ناشر:نشر نمونه|تعداد صفحات:۳۲۰|نوع جلد:شومیز|قطع:رقعی", Label: "عادت‌های اتمی"),
        new("کتاب صد سال تنهایی اثر گابریل گارسیا مارکز", "One Hundred Years of Solitude",
            "book", null, "book", 420_000, 0, 70, None,
            "نویسنده:گابریل گارسیا مارکز|ناشر:نشر نمونه|تعداد صفحات:۴۲۴|نوع جلد:شومیز|قطع:رقعی", Label: "صد سال تنهایی"),
        new("کتاب انسان در جستجوی معنا اثر ویکتور فرانکل", "Man's Search for Meaning",
            "book", null, "book", 260_000, 15, 85, None,
            "نویسنده:ویکتور فرانکل|ناشر:نشر نمونه|تعداد صفحات:۲۰۰|نوع جلد:شومیز|قطع:رقعی", true, "جستجوی معنا"),

        // ---------- Stationery ----------
        new("دفتر یادداشت ۱۰۰ برگ سیمی مدل مینیمال", "Minimal Spiral Notebook 100 sheets",
            "stationery", null, "notebook", 145_000, 0, 200, [Navy, C("سبز", "#047857"), C("زرشکی", "#9f1239")],
            "تعداد برگ:۱۰۰|نوع صحافی:سیمی|نوع کاغذ:تحریر ۷۰ گرمی|قطع:A5", Label: "یادداشت"),

        // ---------- Sports ----------
        new("دمبل ۱۰ کیلوگرمی روکش لاستیکی بسته ۲ عددی", "Rubber Hex Dumbbell 10kg (pair)",
            "sports-gear", null, "dumbbell", 3_200_000, 8, 20, [Black],
            "وزن هر عدد:۱۰ کیلوگرم|جنس:چدن با روکش لاستیک|شکل سر:شش ضلعی|تعداد:۲ عدد"),
        new("توپ فوتبال آدیداس مدل UCL League", "Adidas UCL League Football",
            "sports-gear", "adidas", "ball", 2_150_000, 0, 25, [Blue],
            "سایز:۵|نوع دوخت:حرارتی|مناسب برای:چمن طبیعی و مصنوعی"),
        new("کوله پشتی لپ تاپ شیائومی مدل Business Casual مناسب لپ تاپ ۱۵.۶ اینچی", "Xiaomi Business Casual Backpack",
            "bags", "xiaomi", "backpack", 1_890_000, 12, 33, [Gray, Black],
            "حجم:۲۱ لیتر|جنس:پلی‌استر ضد آب|مناسب لپ‌تاپ تا:۱۵.۶ اینچ|تعداد جیب:۸"),
        new("چمدان سامسونایت مدل Magnum Eco سایز متوسط", "Samsonite Magnum Eco Spinner 69cm",
            "bags", "samsonite", "suitcase", 14_500_000, 7, 6, [Navy, Red, Black],
            "ابعاد:۶۹×۴۷×۲۷ سانتی‌متر|جنس:پلی‌پروپیلن بازیافتی|تعداد چرخ:۴ چرخ ۳۶۰ درجه|قفل:TSA"),

        // ---------- Toys ----------
        new("ساختنی لگو مدل Technic Ford Mustang Shelby GT500 کد 42138", "LEGO Technic 42138",
            "toys", "lego", "toy", 9_800_000, 5, 8, None,
            "تعداد قطعات:۵۴۴|رده سنی:۹ سال به بالا|سری:Technic"),
        new("ماشین بازی کنترلی مدل آفرود صخره‌نورد مقیاس ۱:۱۶", "RC Rock Crawler 1:16",
            "toys", null, "car", 2_300_000, 18, 15, [Red, Orange, Green],
            "مقیاس:۱:۱۶|نوع باتری:شارژی|برد کنترل:۵۰ متر|سرعت:۲۵ کیلومتر بر ساعت", true),

        // ---------- Supermarket ----------
        new("دانه قهوه اسپرسو لاوازا مدل Crema e Aroma مقدار ۱ کیلوگرم", "Lavazza Crema e Aroma Beans 1kg",
            "supermarket", "lavazza", "coffee", 2_890_000, 10, 60, None,
            "وزن:۱ کیلوگرم|نوع:دانه کامل|ترکیب:عربیکا و روبوستا|درجه رست:مدیوم", Label: "LAVAZZA"),
        new("عسل طبیعی چند گیاه مقدار ۱ کیلوگرم", "Natural Multi-flower Honey 1kg",
            "supermarket", null, "honey", 1_250_000, 0, 40, None,
            "وزن:۱ کیلوگرم|نوع:چند گیاه|بسته‌بندی:شیشه‌ای")
    ];

    public static readonly Dictionary<string, string> KindColors = new()
    {
        ["perfume"] = "#a16207", ["book"] = "#0f766e", ["tv"] = "#334155", ["toy"] = "#ef4444", ["coffee"] = "#78350f",
        ["honey"] = "#f59e0b", ["dumbbell"] = "#111827"
    };

    public static string Description(ProductSeed p, string? brandName)
    {
        var brand = brandName == null ? "" : $" از برند {brandName}";
        var intro = $"{p.Title}{brand} یکی از محبوب‌ترین انتخاب‌ها در دسته خود است که با کیفیت ساخت بالا و قیمت مناسب، رضایت خریداران را جلب کرده است.";
        var body = p.Kind switch
        {
            "phone" => "این گوشی با نمایشگر باکیفیت، پردازنده قدرتمند و دوربین‌های متنوع، تجربه‌ای روان در اجرای بازی‌ها، عکاسی و کارهای روزمره ارائه می‌دهد. باتری پرظرفیت آن به شما اجازه می‌دهد بدون نگرانی یک روز کامل از گوشی استفاده کنید.",
            "laptop" => "این لپ‌تاپ با ترکیب پردازنده سریع، حافظه SSD و نمایشگر باکیفیت، گزینه‌ای عالی برای کارهای اداری، برنامه‌نویسی، طراحی و سرگرمی است. بدنه خوش‌ساخت و وزن مناسب، حمل آن را آسان کرده است.",
            "tablet" => "این تبلت با صفحه نمایش بزرگ و روشن، برای تماشای فیلم، مطالعه، یادداشت‌برداری و کارهای آموزشی ایده‌آل است.",
            "headphones" or "earbuds" => "کیفیت صدای شفاف، باس قدرتمند و طراحی ارگونومیک این محصول، تجربه شنیداری لذت‌بخشی را برای ساعت‌های طولانی فراهم می‌کند.",
            "watch" or "band" => "با این گجت پوشیدنی می‌توانید فعالیت‌های ورزشی، کیفیت خواب و ضربان قلب خود را پایش کنید و اعلان‌های گوشی را مستقیماً روی مچ دست ببینید.",
            "tshirt" or "hoodie" or "coat" or "shirt" => "دوخت باکیفیت، پارچه نرم و طراحی به‌روز این محصول باعث می‌شود هم راحت باشید و هم شیک به نظر برسید. پیشنهاد می‌کنیم قبل از خرید جدول سایز را بررسی کنید.",
            "book" => "این کتاب از پرفروش‌ترین آثار در نوع خود است و ترجمه روان آن، خواندنش را برای همه علاقه‌مندان لذت‌بخش می‌کند.",
            _ => "این محصول با طراحی کاربردی و مواد اولیه مرغوب ساخته شده و برای استفاده طولانی‌مدت مناسب است. تمامی محصولات بازارچه دارای ضمانت اصالت و ۷ روز ضمانت بازگشت کالا هستند."
        };
        return $"{intro}\n\n{body}";
    }

    public static readonly string[] ReviewTitles =
    [
        "واقعاً ارزش خرید داره", "از خریدم راضی‌ام", "کیفیت عالی", "نسبت به قیمت خوبه", "بسته‌بندی عالی و ارسال سریع",
        "متوسط بود", "انتظار بیشتری داشتم", "پیشنهاد می‌کنم", "بهترین انتخاب در این رنج قیمت", "اصل و باکیفیت"
    ];

    public static readonly string[] ReviewComments =
    [
        "چند هفته‌ست که استفاده می‌کنم و هیچ مشکلی نداشتم. کیفیت ساختش خیلی خوبه و دقیقاً همونی بود که توی توضیحات نوشته بود.",
        "ارسالش خیلی سریع بود و بسته‌بندی هم کاملاً سالم رسید. به نظرم با این قیمت انتخاب خیلی خوبیه.",
        "در کل راضی‌ام ولی اگه یکم قیمتش پایین‌تر بود بهتر می‌شد. کیفیت در حد انتظار بود.",
        "برای دومین بار هست که از بازارچه خرید می‌کنم و باز هم راضی بودم. محصول اصل بود.",
        "نسبت به مدل قبلی که داشتم پیشرفت خیلی خوبی داشته. به دوستام هم پیشنهادش کردم.",
        "یه مقدار دیر رسید ولی خود محصول عالیه. پشتیبانی هم خیلی خوب پاسخگو بود.",
        "خوبه ولی نه به اندازه‌ای که تعریفش رو شنیده بودم. برای استفاده معمولی کافیه."
    ];

    public static readonly string[] CustomerNames =
    [
        "علی رضایی", "مریم احمدی", "رضا محمدی", "زهرا کریمی", "حسین موسوی", "فاطمه حسینی", "امیر جعفری", "سارا نوری",
        "محمد صادقی", "نگار کاظمی", "پویا رحیمی", "الهام شریفی"
    ];
}
