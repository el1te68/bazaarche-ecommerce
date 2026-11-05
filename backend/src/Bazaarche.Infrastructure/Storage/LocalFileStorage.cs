using Bazaarche.Application.Common;
using Microsoft.Extensions.Options;

namespace Bazaarche.Infrastructure.Storage;

public class StorageOptions
{
    public const string Section = "Storage";
    /// <summary>Physical folder for uploads. In Docker this is a mounted volume.</summary>
    public string UploadsPath { get; set; } = "uploads";
    public long MaxFileBytes { get; set; } = 5 * 1024 * 1024;
}

public class LocalFileStorage(IOptions<StorageOptions> options) : IFileStorage
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    public string Root => Path.GetFullPath(options.Value.UploadsPath);

    public async Task<string> SaveAsync(FileUpload file, string folder, CancellationToken ct = default)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext)) throw new AppException("فقط فایل‌های jpg، png، webp و gif مجاز هستند.");
        if (file.Length <= 0) throw new AppException("فایل خالی است.");
        if (file.Length > options.Value.MaxFileBytes)
            throw new AppException($"حجم فایل نباید بیشتر از {options.Value.MaxFileBytes / 1024 / 1024} مگابایت باشد.");

        var safeFolder = string.Concat(folder.Where(c => char.IsLetterOrDigit(c) || c == '-'));
        var directory = Path.Combine(Root, safeFolder);
        Directory.CreateDirectory(directory);
        var name = $"{Guid.NewGuid():N}{ext}";
        await using (var stream = File.Create(Path.Combine(directory, name)))
            await file.Content.CopyToAsync(stream, ct);
        return $"/uploads/{safeFolder}/{name}";
    }

    public void Delete(string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || !url.StartsWith("/uploads/")) return;
        var path = Path.GetFullPath(Path.Combine(Root, url["/uploads/".Length..]));
        if (!path.StartsWith(Root)) return; // path traversal guard
        try { if (File.Exists(path)) File.Delete(path); } catch (IOException) { /* best effort */ }
    }
}
