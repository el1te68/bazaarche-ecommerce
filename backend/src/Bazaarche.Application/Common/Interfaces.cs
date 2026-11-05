using Bazaarche.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Common;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Address> Addresses { get; }
    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductSpecification> ProductSpecifications { get; }
    DbSet<Review> Reviews { get; }
    DbSet<Banner> Banners { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<WishlistItem> WishlistItems { get; }
    DbSet<Coupon> Coupons { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Payment> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICurrentUser
{
    int? UserId { get; }
    bool IsAdmin { get; }
}

public static class CurrentUserExtensions
{
    public static int RequireId(this ICurrentUser user) => user.UserId ?? throw new UnauthorizedAppException();
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IFileStorage
{
    /// <summary>Saves the file and returns its public URL (e.g. /uploads/products/x.webp).</summary>
    Task<string> SaveAsync(FileUpload file, string folder, CancellationToken ct = default);
    void Delete(string? url);
}
