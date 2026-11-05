using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Bazaarche.Application.Common;
using Bazaarche.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Bazaarche.Infrastructure.Security;

/// <summary>Short JWT claim names (inbound claim mapping is disabled in the API).</summary>
public static class AppClaims
{
    public const string UserId = "sub";
    public const string Name = "name";
    public const string Role = "role";
}

public class JwtOptions
{
    public const string Section = "Jwt";
    public string Issuer { get; set; } = "Bazaarche";
    public string Audience { get; set; } = "Bazaarche.Client";
    /// <summary>At least 32 characters. Override in production via Jwt__Key.</summary>
    public string Key { get; set; } = string.Empty;
    public int ExpireDays { get; set; } = 7;
}

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    public (string Token, DateTime ExpiresAt) CreateToken(User user)
    {
        var o = options.Value;
        var expires = DateTime.UtcNow.AddDays(o.ExpireDays);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = o.Issuer,
            Audience = o.Audience,
            Expires = expires,
            Subject = new ClaimsIdentity(
            [
                new Claim(AppClaims.UserId, user.Id.ToString()),
                new Claim(AppClaims.Name, user.FullName),
                new Claim(AppClaims.Role, user.Role.ToString())
            ]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.Key)), SecurityAlgorithms.HmacSha256)
        };
        return (new JsonWebTokenHandler().CreateToken(descriptor), expires);
    }
}

/// <summary>PBKDF2-SHA256 password hashing. Format: {iterations}.{salt}.{hash} (base64).</summary>
public class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations)) return false;
        var salt = Convert.FromBase64String(parts[1]);
        var expected = Convert.FromBase64String(parts[2]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
