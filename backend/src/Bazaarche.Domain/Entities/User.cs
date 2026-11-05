using Bazaarche.Domain.Common;
using Bazaarche.Domain.Enums;

namespace Bazaarche.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsActive { get; set; } = true;

    public List<Address> Addresses { get; set; } = [];
    public List<Order> Orders { get; set; } = [];
    public List<CartItem> CartItems { get; set; } = [];
    public List<WishlistItem> WishlistItems { get; set; } = [];
}

public class Address : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string FullAddress { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
