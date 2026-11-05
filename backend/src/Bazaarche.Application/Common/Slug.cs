using System.Text;

namespace Bazaarche.Application.Common;

public static class Slug
{
    /// <summary>Builds a URL slug. Persian/Arabic letters are kept, so "گوشی سامسونگ A55" becomes "گوشی-سامسونگ-a55".</summary>
    public static string Generate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Guid.NewGuid().ToString("N")[..8];
        var normalized = text.Trim().ToLowerInvariant()
            .Replace('ي', 'ی').Replace('ك', 'ک').Replace('‌', '-');
        var sb = new StringBuilder();
        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch)) sb.Append(ch);
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        var result = sb.ToString().Trim('-');
        return result.Length == 0 ? Guid.NewGuid().ToString("N")[..8] : result.Length > 120 ? result[..120].Trim('-') : result;
    }

    /// <summary>Appends -2, -3, ... until <paramref name="exists"/> returns false.</summary>
    public static async Task<string> UniqueAsync(string baseSlug, Func<string, Task<bool>> exists)
    {
        var slug = baseSlug;
        var i = 2;
        while (await exists(slug)) slug = $"{baseSlug}-{i++}";
        return slug;
    }
}
