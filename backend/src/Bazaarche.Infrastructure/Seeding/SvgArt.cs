using System.Globalization;
using System.Security;

namespace Bazaarche.Infrastructure.Seeding;

/// <summary>
/// Generates clean, flat product illustrations (SVG) for the demo catalog, so the project runs
/// without any external image assets.
/// </summary>
public static class SvgArt
{
    private static string Esc(string s) => SecurityElement.Escape(s) ?? string.Empty;

    private static string Shade(string hex, double factor)
    {
        hex = hex.TrimStart('#');
        int r = Convert.ToInt32(hex[..2], 16), g = Convert.ToInt32(hex[2..4], 16), b = Convert.ToInt32(hex[4..6], 16);
        int Mix(int c) => factor >= 0 ? (int)(c + (255 - c) * factor) : (int)(c * (1 + factor));
        return $"#{Math.Clamp(Mix(r), 0, 255):x2}{Math.Clamp(Mix(g), 0, 255):x2}{Math.Clamp(Mix(b), 0, 255):x2}";
    }

    public static string Product(string kind, string color, int variant, string label)
    {
        var isLight = IsLight(color);
        var bg1 = isLight ? "#f1f5f9" : variant == 0 ? "#ffffff" : Shade(color, 0.92);
        var bg2 = isLight ? "#cbd5e1" : variant == 0 ? "#f3f4f6" : Shade(color, 0.8);
        var light = Shade(color, 0.35);
        var dark = Shade(color, -0.35);
        var body = Shape(kind, color, light, dark, label);
        var rotate = variant == 2 ? " transform=\"rotate(-12 300 300)\"" : "";
        return $"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 600" width="600" height="600">
  <defs>
    <radialGradient id="bg" cx="50%" cy="40%" r="75%"><stop offset="0" stop-color="{bg1}"/><stop offset="1" stop-color="{bg2}"/></radialGradient>
    <linearGradient id="gl" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#ffffff" stop-opacity=".55"/><stop offset=".5" stop-color="#ffffff" stop-opacity="0"/></linearGradient>
    <linearGradient id="scr" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#1e3a8a"/><stop offset=".55" stop-color="#7c3aed"/><stop offset="1" stop-color="#db2777"/></linearGradient>
  </defs>
  <rect width="600" height="600" fill="url(#bg)"/>
  <ellipse cx="300" cy="540" rx="190" ry="18" fill="#000" opacity=".07"/>
  <g{rotate}>{body}</g>
</svg>
""";
    }

    private static bool IsLight(string hex)
    {
        hex = hex.TrimStart('#');
        int r = Convert.ToInt32(hex[..2], 16), g = Convert.ToInt32(hex[2..4], 16), b = Convert.ToInt32(hex[4..6], 16);
        return (0.299 * r + 0.587 * g + 0.114 * b) / 255 > 0.8;
    }

    private static string Shape(string kind, string c, string light, string dark, string label)
    {
        var txt = IsLight(c) ? "#334155" : "#ffffff";
        return kind switch
    {
        "phone" => $"""
            <rect x="185" y="70" width="230" height="460" rx="40" fill="{dark}"/>
            <rect x="195" y="80" width="210" height="440" rx="32" fill="url(#scr)"/>
            <rect x="265" y="94" width="70" height="16" rx="8" fill="#0b0b0b"/>
            <circle cx="300" cy="300" r="60" fill="#fff" opacity=".18"/>
            <rect x="195" y="80" width="210" height="440" rx="32" fill="url(#gl)"/>
            <rect x="415" y="170" width="6" height="60" rx="3" fill="{dark}"/>
            """,
        "earbuds" => $"""
            <rect x="170" y="250" width="260" height="200" rx="90" fill="{c}"/>
            <rect x="170" y="250" width="260" height="70" rx="35" fill="{light}"/>
            <line x1="175" y1="322" x2="425" y2="322" stroke="{dark}" stroke-width="4"/>
            <circle cx="300" cy="390" r="7" fill="#22c55e"/>
            <g transform="translate(205 120)"><circle cx="40" cy="40" r="38" fill="{light}"/><rect x="28" y="60" width="24" height="90" rx="12" fill="{light}"/></g>
            <g transform="translate(315 120)"><circle cx="40" cy="40" r="38" fill="{light}"/><rect x="28" y="60" width="24" height="90" rx="12" fill="{light}"/></g>
            """,
        "headphones" => $"""
            <path d="M150 330 C150 150 450 150 450 330" fill="none" stroke="{dark}" stroke-width="30" stroke-linecap="round"/>
            <rect x="115" y="290" width="95" height="170" rx="45" fill="{c}"/>
            <rect x="390" y="290" width="95" height="170" rx="45" fill="{c}"/>
            <rect x="150" y="310" width="40" height="130" rx="20" fill="{light}"/>
            <rect x="410" y="310" width="40" height="130" rx="20" fill="{light}"/>
            """,
        "powerbank" or "charger" => $"""
            <rect x="200" y="{(kind == "charger" ? 200 : 110)}" width="200" height="{(kind == "charger" ? 230 : 380)}" rx="34" fill="{c}"/>
            <rect x="200" y="{(kind == "charger" ? 200 : 110)}" width="200" height="{(kind == "charger" ? 230 : 380)}" rx="34" fill="url(#gl)"/>
            <rect x="265" y="{(kind == "charger" ? 160 : 470)}" width="70" height="{(kind == "charger" ? 40 : 14)}" rx="6" fill="{dark}"/>
            <circle cx="260" cy="{(kind == "charger" ? 380 : 430)}" r="7" fill="#22c55e"/><circle cx="285" cy="{(kind == "charger" ? 380 : 430)}" r="7" fill="#22c55e"/><circle cx="310" cy="{(kind == "charger" ? 380 : 430)}" r="7" fill="#22c55e"/>
            <text x="300" y="{(kind == "charger" ? 320 : 300)}" font-family="Arial, sans-serif" font-size="34" font-weight="700" fill="{txt}" text-anchor="middle">{Esc(label)}</text>
            """,
        "laptop" => $"""
            <rect x="130" y="130" width="340" height="230" rx="16" fill="{dark}"/>
            <rect x="145" y="145" width="310" height="200" rx="6" fill="url(#scr)"/>
            <rect x="145" y="145" width="310" height="200" rx="6" fill="url(#gl)"/>
            <path d="M90 372 H510 L490 410 H110 Z" fill="{c}"/>
            <rect x="265" y="372" width="70" height="9" rx="4" fill="{dark}"/>
            """,
        "tablet" => $"""
            <rect x="150" y="100" width="300" height="400" rx="30" fill="{dark}"/>
            <rect x="163" y="113" width="274" height="374" rx="20" fill="url(#scr)"/>
            <rect x="163" y="113" width="274" height="374" rx="20" fill="url(#gl)"/>
            <circle cx="300" cy="106" r="3" fill="#333"/>
            """,
        "watch" or "band" => $"""
            <rect x="{(kind == "band" ? 268 : 245)}" y="80" width="{(kind == "band" ? 64 : 110)}" height="440" rx="30" fill="{c}"/>
            <rect x="{(kind == "band" ? 255 : 205)}" y="{(kind == "band" ? 230 : 205)}" width="{(kind == "band" ? 90 : 190)}" height="{(kind == "band" ? 140 : 190)}" rx="{(kind == "band" ? 40 : 52)}" fill="#111827"/>
            <rect x="{(kind == "band" ? 263 : 217)}" y="{(kind == "band" ? 240 : 217)}" width="{(kind == "band" ? 74 : 166)}" height="{(kind == "band" ? 120 : 166)}" rx="{(kind == "band" ? 34 : 44)}" fill="url(#scr)"/>
            <text x="300" y="{(kind == "band" ? 308 : 312)}" font-family="Arial, sans-serif" font-size="{(kind == "band" ? 26 : 40)}" font-weight="700" fill="#fff" text-anchor="middle">10:09</text>
            {(kind == "watch" ? $"<rect x=\"393\" y=\"275\" width=\"14\" height=\"50\" rx=\"6\" fill=\"{dark}\"/>" : "")}
            """,
        "speaker" => $"""
            <rect x="120" y="215" width="360" height="170" rx="85" fill="{c}"/>
            <rect x="140" y="230" width="320" height="140" rx="70" fill="{dark}" opacity=".35"/>
            <circle cx="210" cy="300" r="48" fill="{light}"/><circle cx="210" cy="300" r="20" fill="{dark}"/>
            <circle cx="390" cy="300" r="48" fill="{light}"/><circle cx="390" cy="300" r="20" fill="{dark}"/>
            <text x="300" y="312" font-family="Arial, sans-serif" font-size="30" font-weight="800" fill="{txt}" text-anchor="middle">{Esc(label)}</text>
            """,
        "camera" => $"""
            <rect x="120" y="200" width="360" height="230" rx="28" fill="#1f2937"/>
            <rect x="160" y="165" width="110" height="45" rx="10" fill="#1f2937"/>
            <circle cx="320" cy="315" r="95" fill="#374151"/><circle cx="320" cy="315" r="70" fill="#111827"/>
            <circle cx="320" cy="315" r="45" fill="url(#scr)"/><circle cx="300" cy="295" r="12" fill="#fff" opacity=".6"/>
            <circle cx="440" cy="235" r="10" fill="{c}"/>
            <text x="175" y="250" font-family="Arial, sans-serif" font-size="22" font-weight="700" fill="#e5e7eb">{Esc(label)}</text>
            """,
        "tv" => $"""
            <rect x="70" y="120" width="460" height="290" rx="12" fill="#0f172a"/>
            <rect x="82" y="132" width="436" height="266" rx="4" fill="url(#scr)"/>
            <rect x="82" y="132" width="436" height="266" rx="4" fill="url(#gl)"/>
            <path d="M250 410 L230 470 H370 L350 410 Z" fill="{dark}"/>
            <rect x="200" y="468" width="200" height="12" rx="6" fill="{dark}"/>
            """,
        "airfryer" or "coffee-maker" or "kettle" or "blender" => Appliance(kind, c, light, dark),
        "robot" => $"""
            <ellipse cx="300" cy="330" rx="190" ry="120" fill="{dark}"/>
            <ellipse cx="300" cy="310" rx="190" ry="120" fill="{c}"/>
            <circle cx="300" cy="270" r="45" fill="{light}"/><circle cx="300" cy="270" r="18" fill="{dark}"/>
            <rect x="200" y="360" width="200" height="12" rx="6" fill="{light}"/>
            """,
        "vacuum" => $"""
            <rect x="285" y="70" width="30" height="300" rx="15" fill="{dark}"/>
            <rect x="230" y="200" width="140" height="190" rx="50" fill="{c}"/>
            <rect x="250" y="230" width="100" height="70" rx="20" fill="{light}" opacity=".7"/>
            <rect x="190" y="420" width="220" height="50" rx="20" fill="{dark}"/>
            <rect x="290" y="380" width="20" height="45" fill="{dark}"/>
            """,
        "tshirt" or "hoodie" or "coat" or "shirt" => Garment(kind, c, light, dark),
        "shoe" => $"""
            <path d="M90 380 C90 300 160 300 200 260 C230 230 270 240 290 270 C330 330 420 320 500 350 C530 362 520 420 500 430 L110 430 C95 430 90 410 90 380 Z" fill="{c}"/>
            <path d="M90 405 H515 C512 425 505 440 490 440 H110 C96 440 90 425 90 405 Z" fill="#ffffff"/>
            <path d="M210 280 L240 320 M235 265 L265 305 M260 262 L290 300" stroke="#fff" stroke-width="8" stroke-linecap="round"/>
            <path d="M330 330 C380 345 430 350 470 360" stroke="{light}" stroke-width="10" fill="none" stroke-linecap="round"/>
            """,
        "perfume" => $"""
            <rect x="265" y="110" width="70" height="60" rx="8" fill="#d4af37"/>
            <rect x="285" y="165" width="30" height="30" fill="#b8962e"/>
            <rect x="185" y="190" width="230" height="300" rx="34" fill="{c}" opacity=".9"/>
            <rect x="185" y="190" width="230" height="300" rx="34" fill="url(#gl)"/>
            <rect x="225" y="300" width="150" height="80" rx="6" fill="#fff" opacity=".85"/>
            <text x="300" y="350" font-family="Georgia, serif" font-size="26" fill="#111" text-anchor="middle">{Esc(label)}</text>
            """,
        "shaver" => $"""
            <rect x="240" y="150" width="120" height="360" rx="60" fill="{c}"/>
            <rect x="225" y="95" width="150" height="80" rx="20" fill="{dark}"/>
            <path d="M235 120 H365 M235 140 H365" stroke="{light}" stroke-width="6"/>
            <circle cx="300" cy="300" r="22" fill="{light}"/>
            """,
        "book" or "notebook" => $"""
            <rect x="175" y="95" width="270" height="400" rx="8" fill="{dark}"/>
            <rect x="160" y="85" width="270" height="400" rx="8" fill="{c}"/>
            <rect x="160" y="85" width="28" height="400" fill="{dark}" opacity=".5"/>
            <rect x="215" y="170" width="180" height="2" fill="#fff" opacity=".7"/>
            <text x="305" y="250" font-family="Tahoma, Vazirmatn, sans-serif" font-size="{(label.Length > 12 ? 24 : 30)}" font-weight="700" fill="#fff" text-anchor="middle" direction="rtl">{Esc(label)}</text>
            <rect x="215" y="300" width="180" height="2" fill="#fff" opacity=".7"/>
            """,
        "dumbbell" => $"""
            <rect x="150" y="285" width="300" height="30" rx="15" fill="#9ca3af"/>
            <rect x="110" y="210" width="60" height="180" rx="16" fill="{c}"/><rect x="430" y="210" width="60" height="180" rx="16" fill="{c}"/>
            <rect x="170" y="235" width="35" height="130" rx="12" fill="{dark}"/><rect x="395" y="235" width="35" height="130" rx="12" fill="{dark}"/>
            """,
        "ball" => $"""
            <circle cx="300" cy="300" r="175" fill="#fff" stroke="#d1d5db" stroke-width="4"/>
            <polygon points="300,230 360,272 337,342 263,342 240,272" fill="{c}"/>
            <path d="M300 230 V140 M360 272 L440 245 M337 342 L385 420 M263 342 L215 420 M240 272 L160 245" stroke="{c}" stroke-width="8"/>
            """,
        "backpack" or "suitcase" => kind == "backpack" ? $"""
            <path d="M260 110 C260 80 340 80 340 110" fill="none" stroke="{dark}" stroke-width="16"/>
            <rect x="180" y="110" width="240" height="380" rx="70" fill="{c}"/>
            <rect x="215" y="300" width="170" height="150" rx="34" fill="{light}"/>
            <rect x="240" y="320" width="120" height="8" rx="4" fill="{dark}"/>
            """ : $"""
            <path d="M255 140 V100 H345 V140" fill="none" stroke="{dark}" stroke-width="14"/>
            <rect x="185" y="140" width="230" height="350" rx="30" fill="{c}"/>
            <path d="M245 160 V470 M300 160 V470 M355 160 V470" stroke="{dark}" stroke-width="6" opacity=".4"/>
            <circle cx="220" cy="505" r="14" fill="#111"/><circle cx="380" cy="505" r="14" fill="#111"/>
            """,
        "toy" => $"""
            <rect x="150" y="320" width="140" height="140" rx="12" fill="{c}"/><rect x="310" y="320" width="140" height="140" rx="12" fill="#f59e0b"/>
            <rect x="230" y="170" width="140" height="140" rx="12" fill="#3b82f6"/>
            <g fill="#fff" opacity=".45"><circle cx="190" cy="320" r="18"/><circle cx="250" cy="320" r="18"/><circle cx="350" cy="320" r="18"/><circle cx="410" cy="320" r="18"/><circle cx="270" cy="170" r="18"/><circle cx="330" cy="170" r="18"/></g>
            """,
        "car" => $"""
            <path d="M110 380 L140 300 C150 275 170 265 200 265 H380 C410 265 430 280 445 300 L490 340 C505 350 510 365 510 380 V410 H110 Z" fill="{c}"/>
            <path d="M190 285 H290 V335 H165 Z M310 285 H380 C395 285 405 292 415 305 L435 335 H310 Z" fill="#bae6fd"/>
            <circle cx="200" cy="415" r="42" fill="#111827"/><circle cx="200" cy="415" r="18" fill="#9ca3af"/>
            <circle cx="420" cy="415" r="42" fill="#111827"/><circle cx="420" cy="415" r="18" fill="#9ca3af"/>
            """,
        "coffee" => $"""
            <path d="M200 140 H400 L420 490 H180 Z" fill="{c}"/>
            <rect x="200" y="120" width="200" height="30" rx="4" fill="{dark}"/>
            <ellipse cx="300" cy="320" rx="70" ry="50" fill="{light}"/>
            <path d="M300 275 C280 300 320 340 300 365" stroke="{dark}" stroke-width="8" fill="none"/>
            <text x="300" y="430" font-family="Arial, sans-serif" font-size="28" font-weight="800" fill="#fff" text-anchor="middle">{Esc(label)}</text>
            """,
        "honey" => $"""
            <rect x="225" y="130" width="150" height="50" rx="10" fill="#7c2d12"/>
            <rect x="190" y="175" width="220" height="310" rx="50" fill="#f59e0b"/>
            <rect x="190" y="175" width="220" height="310" rx="50" fill="url(#gl)"/>
            <polygon points="300,280 345,305 345,355 300,380 255,355 255,305" fill="#fff" opacity=".85"/>
            """,
        _ => $"""
            <rect x="160" y="160" width="280" height="280" rx="30" fill="{c}"/>
            <rect x="160" y="160" width="280" height="280" rx="30" fill="url(#gl)"/>
            <text x="300" y="315" font-family="Arial, sans-serif" font-size="32" font-weight="700" fill="#fff" text-anchor="middle">{Esc(label)}</text>
            """
    };
    }

    private static string Appliance(string kind, string c, string light, string dark) => kind switch
    {
        "airfryer" => $"""
            <rect x="180" y="110" width="240" height="380" rx="60" fill="{c}"/>
            <rect x="200" y="150" width="200" height="70" rx="20" fill="#111827"/>
            <text x="300" y="196" font-family="Arial, sans-serif" font-size="30" font-weight="700" fill="#38bdf8" text-anchor="middle">200°</text>
            <rect x="200" y="260" width="200" height="200" rx="30" fill="{dark}"/>
            <rect x="260" y="330" width="80" height="18" rx="9" fill="{light}"/>
            """,
        "coffee-maker" => $"""
            <rect x="170" y="100" width="260" height="90" rx="20" fill="{dark}"/>
            <rect x="170" y="180" width="70" height="300" fill="{c}"/>
            <rect x="170" y="460" width="260" height="40" rx="10" fill="{dark}"/>
            <rect x="260" y="200" width="110" height="40" rx="10" fill="#9ca3af"/>
            <path d="M285 360 H360 V430 C360 445 350 455 335 455 H310 C295 455 285 445 285 430 Z" fill="#fff"/>
            <path d="M360 375 C385 375 385 420 360 420" stroke="#fff" stroke-width="8" fill="none"/>
            """,
        "kettle" => $"""
            <path d="M200 190 H400 L420 470 H180 Z" fill="{c}"/>
            <path d="M200 190 C210 140 390 140 400 190 Z" fill="{dark}"/>
            <path d="M400 230 C470 240 470 380 410 400" stroke="{dark}" stroke-width="22" fill="none"/>
            <path d="M200 230 L140 200" stroke="{c}" stroke-width="26" stroke-linecap="round"/>
            <rect x="170" y="470" width="260" height="30" rx="10" fill="{dark}"/>
            """,
        _ => $"""
            <path d="M210 110 H390 L370 360 H230 Z" fill="#e0f2fe" stroke="#94a3b8" stroke-width="6"/>
            <rect x="200" y="90" width="200" height="30" rx="10" fill="{dark}"/>
            <rect x="200" y="360" width="200" height="140" rx="26" fill="{c}"/>
            <circle cx="300" cy="430" r="26" fill="{light}"/>
            """
    };

    private static string Garment(string kind, string c, string light, string dark)
    {
        var length = kind is "coat" ? 500 : 460;
        var collar = kind switch
        {
            "hoodie" => $"<path d=\"M240 110 C240 40 360 40 360 110\" fill=\"{dark}\"/><path d=\"M285 130 V200 M315 130 V200\" stroke=\"#fff\" stroke-width=\"5\"/><rect x=\"240\" y=\"330\" width=\"120\" height=\"70\" rx=\"10\" fill=\"{dark}\" opacity=\".35\"/>",
            "coat" => $"<path d=\"M300 110 V{length}\" stroke=\"{dark}\" stroke-width=\"6\"/><circle cx=\"315\" cy=\"200\" r=\"7\" fill=\"{dark}\"/><circle cx=\"315\" cy=\"270\" r=\"7\" fill=\"{dark}\"/><circle cx=\"315\" cy=\"340\" r=\"7\" fill=\"{dark}\"/><circle cx=\"315\" cy=\"410\" r=\"7\" fill=\"{dark}\"/>",
            "shirt" => $"<path d=\"M262 105 L300 150 L338 105\" fill=\"none\" stroke=\"{dark}\" stroke-width=\"8\"/><path d=\"M300 150 V{length}\" stroke=\"{dark}\" stroke-width=\"4\"/>",
            _ => $"<path d=\"M262 105 C270 140 330 140 338 105\" fill=\"none\" stroke=\"{dark}\" stroke-width=\"10\"/>"
        };
        var sleeveEnd = kind is "tshirt" ? 250 : 400;
        return $"""
            <path d="M240 100 L150 140 L100 {sleeveEnd - 60} L160 {sleeveEnd} L195 {sleeveEnd - 120} L195 {length} H405 L405 {sleeveEnd - 120} L440 {sleeveEnd} L500 {sleeveEnd - 60} L450 140 L360 100 C340 125 260 125 240 100 Z" fill="{c}"/>
            <path d="M195 {length - 25} H405" stroke="{light}" stroke-width="8"/>
            {collar}
            """;
    }

    public static string Banner(string title, string subtitle, string from, string to, string cta)
    {
        return $"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 1600 480" width="1600" height="480">
  <defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="{from}"/><stop offset="1" stop-color="{to}"/></linearGradient></defs>
  <rect width="1600" height="480" fill="url(#g)"/>
  <circle cx="220" cy="80" r="260" fill="#fff" opacity=".07"/>
  <circle cx="420" cy="460" r="180" fill="#fff" opacity=".06"/>
  <circle cx="1450" cy="420" r="220" fill="#000" opacity=".06"/>
  <g fill="#fff" opacity=".12"><rect x="160" y="140" width="120" height="220" rx="22"/><rect x="310" y="190" width="170" height="120" rx="14"/><circle cx="560" cy="250" r="60"/></g>
  <text x="1440" y="210" font-family="Tahoma, Vazirmatn, sans-serif" font-size="78" font-weight="700" fill="#fff" text-anchor="start" direction="rtl">{Esc(title)}</text>
  <text x="1440" y="285" font-family="Tahoma, Vazirmatn, sans-serif" font-size="36" fill="#fff" opacity=".9" text-anchor="start" direction="rtl">{Esc(subtitle)}</text>
  <rect x="1180" y="320" width="260" height="70" rx="35" fill="#fff"/>
  <text x="1310" y="366" font-family="Tahoma, Vazirmatn, sans-serif" font-size="30" font-weight="700" fill="{to}" text-anchor="middle" direction="rtl">{Esc(cta)}</text>
</svg>
""";
    }

    public static string SmallBanner(string title, string subtitle, string from, string to)
    {
        return $"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 800 400" width="800" height="400">
  <defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="{from}"/><stop offset="1" stop-color="{to}"/></linearGradient></defs>
  <rect width="800" height="400" rx="0" fill="url(#g)"/>
  <circle cx="120" cy="320" r="170" fill="#fff" opacity=".1"/>
  <circle cx="160" cy="300" r="90" fill="#fff" opacity=".12"/>
  <text x="740" y="170" font-family="Tahoma, Vazirmatn, sans-serif" font-size="56" font-weight="700" fill="#fff" text-anchor="start" direction="rtl">{Esc(title)}</text>
  <text x="740" y="235" font-family="Tahoma, Vazirmatn, sans-serif" font-size="30" fill="#fff" opacity=".9" text-anchor="start" direction="rtl">{Esc(subtitle)}</text>
</svg>
""";
    }

    public static string BrandLogo(string englishName, string color)
    {
        var size = englishName.Length > 8 ? 34 : 44;
        return $"""
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 300 150" width="300" height="150">
  <rect width="300" height="150" fill="#ffffff"/>
  <text x="150" y="{(75 + size / 3).ToString(CultureInfo.InvariantCulture)}" font-family="Arial Black, Arial, sans-serif" font-size="{size}" font-weight="900" fill="{color}" text-anchor="middle" letter-spacing="1">{Esc(englishName.ToUpperInvariant())}</text>
</svg>
""";
    }
}
