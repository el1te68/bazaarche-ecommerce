using System.ComponentModel.DataAnnotations;
using Bazaarche.Application.Common;
using Bazaarche.Domain.Entities;
using Bazaarche.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Bazaarche.Application.Features.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "نام و نام خانوادگی الزامی است.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "نام باید بین ۳ تا ۸۰ کاراکتر باشد.")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است.")]
    [RegularExpression(@"^09\d{9}$", ErrorMessage = "شماره موبایل معتبر نیست (مثال: 09121234567).")]
    public string PhoneNumber { get; init; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    public string? Email { get; init; }

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [MinLength(6, ErrorMessage = "رمز عبور حداقل ۶ کاراکتر باشد.")]
    public string Password { get; init; } = string.Empty;
}

public class LoginRequest
{
    [Required(ErrorMessage = "شماره موبایل یا ایمیل را وارد کنید.")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور را وارد کنید.")]
    public string Password { get; init; } = string.Empty;
}

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "نام الزامی است.")]
    [StringLength(80, MinimumLength = 3, ErrorMessage = "نام باید بین ۳ تا ۸۰ کاراکتر باشد.")]
    public string FullName { get; init; } = string.Empty;

    [EmailAddress(ErrorMessage = "ایمیل معتبر نیست.")]
    public string? Email { get; init; }
}

public class ChangePasswordRequest
{
    [Required] public string CurrentPassword { get; init; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "رمز عبور جدید حداقل ۶ کاراکتر باشد.")]
    public string NewPassword { get; init; } = string.Empty;
}

public record UserDto(int Id, string FullName, string PhoneNumber, string? Email, UserRole Role, DateTime CreatedAt);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public class AuthService(IAppDbContext db, IPasswordHasher hasher, ITokenService tokens, ICurrentUser currentUser)
{
    public static UserDto ToDto(User u) => new(u.Id, u.FullName, u.PhoneNumber, u.Email, u.Role, u.CreatedAt);

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var phone = request.PhoneNumber.Trim();
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.PhoneNumber == phone, ct))
            throw new AppException("این شماره موبایل قبلاً ثبت‌نام کرده است.");
        if (email != null && await db.Users.AnyAsync(u => u.Email == email, ct))
            throw new AppException("این ایمیل قبلاً ثبت شده است.");

        var user = new User
        {
            FullName = request.FullName.Trim(),
            PhoneNumber = phone,
            Email = email,
            PasswordHash = hasher.Hash(request.Password),
            Role = UserRole.Customer
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return Issue(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var username = request.Username.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == username || u.Email == username, ct);
        if (user == null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new AppException("نام کاربری یا رمز عبور اشتباه است.", 401);
        if (!user.IsActive)
            throw new AppException("حساب کاربری شما غیرفعال شده است.", 403);
        return Issue(user);
    }

    public async Task<UserDto> MeAsync(CancellationToken ct)
    {
        var id = currentUser.RequireId();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new UnauthorizedAppException();
        return ToDto(user);
    }

    public async Task<UserDto> UpdateProfileAsync(UpdateProfileRequest request, CancellationToken ct)
    {
        var id = currentUser.RequireId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new UnauthorizedAppException();
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim().ToLowerInvariant();
        if (email != null && await db.Users.AnyAsync(u => u.Email == email && u.Id != id, ct))
            throw new AppException("این ایمیل قبلاً ثبت شده است.");
        user.FullName = request.FullName.Trim();
        user.Email = email;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return ToDto(user);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct)
    {
        var id = currentUser.RequireId();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct) ?? throw new UnauthorizedAppException();
        if (!hasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new AppException("رمز عبور فعلی اشتباه است.");
        user.PasswordHash = hasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    private AuthResponse Issue(User user)
    {
        var (token, expires) = tokens.CreateToken(user);
        return new AuthResponse(token, expires, ToDto(user));
    }
}
