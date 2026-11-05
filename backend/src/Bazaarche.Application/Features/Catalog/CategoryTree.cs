using Bazaarche.Application.Common;
using Bazaarche.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Catalog;

/// <summary>In-memory view of the (small) category table: tree building, breadcrumbs and descendants.</summary>
public class CategoryTree
{
    private readonly List<Category> _all;
    private readonly Dictionary<int, Category> _byId;

    private CategoryTree(List<Category> all)
    {
        _all = all;
        _byId = all.ToDictionary(c => c.Id);
    }

    public static async Task<CategoryTree> LoadAsync(IAppDbContext db, bool activeOnly, CancellationToken ct)
    {
        var query = db.Categories.AsNoTracking();
        if (activeOnly) query = query.Where(c => c.IsActive);
        return new CategoryTree(await query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id).ToListAsync(ct));
    }

    public Category? FindBySlug(string slug) => _all.FirstOrDefault(c => c.Slug == slug);

    public Category? Find(int id) => _byId.GetValueOrDefault(id);

    public List<CategoryNodeDto> Roots() => _all.Where(c => c.ParentId == null || !_byId.ContainsKey(c.ParentId.Value)).Select(ToNode).ToList();

    public CategoryNodeDto ToNode(Category c) =>
        new(c.Id, c.Name, c.Slug, c.Icon, c.ImageUrl, _all.Where(x => x.ParentId == c.Id).Select(ToNode).ToList());

    public List<int> DescendantIdsIncludingSelf(int id)
    {
        var result = new List<int> { id };
        var queue = new Queue<int>([id]);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var child in _all.Where(c => c.ParentId == current))
            {
                result.Add(child.Id);
                queue.Enqueue(child.Id);
            }
        }
        return result;
    }

    public List<CategoryLinkDto> Breadcrumbs(int id)
    {
        var list = new List<CategoryLinkDto>();
        var guard = 0;
        var current = Find(id);
        while (current != null && guard++ < 20)
        {
            list.Insert(0, new CategoryLinkDto(current.Id, current.Name, current.Slug));
            current = current.ParentId.HasValue ? Find(current.ParentId.Value) : null;
        }
        return list;
    }
}
