namespace Bazaarche.Application.Common;

public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

/// <summary>An uploaded file, decoupled from ASP.NET's IFormFile.</summary>
public record FileUpload(Stream Content, string FileName, long Length);

public static class Paging
{
    public static (int page, int pageSize) Normalize(int page, int pageSize, int max = 60)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > max) pageSize = max;
        return (page, pageSize);
    }
}
