using Bazaarche.Application.Common;
using Bazaarche.Domain.Common;
using Bazaarche.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.Property(x => x.FullName).HasMaxLength(80).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(11).IsRequired();
            e.Property(x => x.Email).HasMaxLength(120);
            e.Property(x => x.PasswordHash).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.PhoneNumber).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        b.Entity<Address>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(40);
            e.Property(x => x.ReceiverName).HasMaxLength(80);
            e.Property(x => x.ReceiverPhone).HasMaxLength(11);
            e.Property(x => x.Province).HasMaxLength(40);
            e.Property(x => x.City).HasMaxLength(40);
            e.Property(x => x.FullAddress).HasMaxLength(400);
            e.Property(x => x.PostalCode).HasMaxLength(10);
            e.HasOne(x => x.User).WithMany(u => u.Addresses).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Category>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            e.Property(x => x.Icon).HasMaxLength(40);
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Brand>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(80).IsRequired();
            e.Property(x => x.EnglishName).HasMaxLength(80).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();
        });

        b.Entity<Product>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.EnglishTitle).HasMaxLength(200);
            e.Property(x => x.Slug).HasMaxLength(140).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();
            e.HasIndex(x => x.FinalPrice);
            e.HasIndex(x => x.SoldCount);
            e.HasIndex(x => x.CreatedAt);
            e.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Brand).WithMany(c => c.Products).HasForeignKey(x => x.BrandId).OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<ProductImage>(e =>
        {
            e.Property(x => x.Url).HasMaxLength(300).IsRequired();
            e.HasOne(x => x.Product).WithMany(p => p.Images).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ProductVariant>(e =>
        {
            e.Property(x => x.ColorName).HasMaxLength(40);
            e.Property(x => x.ColorHex).HasMaxLength(7);
            e.Property(x => x.Sku).HasMaxLength(60);
            e.HasOne(x => x.Product).WithMany(p => p.Variants).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<ProductSpecification>(e =>
        {
            e.Property(x => x.Key).HasMaxLength(80);
            e.Property(x => x.Value).HasMaxLength(300);
            e.HasOne(x => x.Product).WithMany(p => p.Specifications).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Review>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(100);
            e.Property(x => x.Comment).HasMaxLength(2000);
            e.HasIndex(x => new { x.ProductId, x.UserId }).IsUnique();
            e.HasOne(x => x.Product).WithMany(p => p.Reviews).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Banner>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(120);
            e.Property(x => x.ImageUrl).HasMaxLength(300);
            e.Property(x => x.LinkUrl).HasMaxLength(300);
        });

        b.Entity<CartItem>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.ProductVariantId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.CartItems).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ProductVariant).WithMany().HasForeignKey(x => x.ProductVariantId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<WishlistItem>(e =>
        {
            e.HasIndex(x => new { x.UserId, x.ProductId }).IsUnique();
            e.HasOne(x => x.User).WithMany(u => u.WishlistItems).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Coupon>(e =>
        {
            e.Property(x => x.Code).HasMaxLength(30).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        b.Entity<Order>(e =>
        {
            e.Property(x => x.OrderNumber).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.OrderNumber).IsUnique();
            e.HasIndex(x => x.Status);
            e.Property(x => x.ReceiverName).HasMaxLength(80);
            e.Property(x => x.ReceiverPhone).HasMaxLength(11);
            e.Property(x => x.Province).HasMaxLength(40);
            e.Property(x => x.City).HasMaxLength(40);
            e.Property(x => x.FullAddress).HasMaxLength(400);
            e.Property(x => x.PostalCode).HasMaxLength(10);
            e.Property(x => x.CouponCode).HasMaxLength(30);
            e.Property(x => x.TrackingCode).HasMaxLength(60);
            e.HasOne(x => x.User).WithMany(u => u.Orders).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<OrderItem>(e =>
        {
            // Order items are a snapshot: no FK to products, so products can be deleted without losing order history.
            e.Property(x => x.ProductTitle).HasMaxLength(200);
            e.Property(x => x.ProductSlug).HasMaxLength(140);
            e.Property(x => x.ImageUrl).HasMaxLength(300);
            e.Property(x => x.ColorName).HasMaxLength(40);
            e.HasIndex(x => x.ProductId);
            e.HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Payment>(e =>
        {
            e.Property(x => x.Token).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();
            e.Property(x => x.ReferenceId).HasMaxLength(30);
            e.Property(x => x.CardMask).HasMaxLength(20);
            e.HasOne(x => x.Order).WithMany(o => o.Payments).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            if (entry.State == EntityState.Modified) entry.Entity.UpdatedAt = DateTime.UtcNow;
        return base.SaveChangesAsync(cancellationToken);
    }
}
