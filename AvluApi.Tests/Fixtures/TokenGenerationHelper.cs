using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AvluApi.Tests.Fixtures;

/// <summary>
/// Helper class to generate JWT tokens for testing
/// </summary>
public static class TokenGenerationHelper
{
    private const string DefaultKey = "AvluApiSuperSecretJwtKey2024XYZ!@#$%";
    private const string DefaultIssuer = "AvluApi";
    private const string DefaultAudience = "AvluApiClients";

    /// <summary>
    /// Generate a valid JWT token with claims
    /// </summary>
    public static string GenerateToken(
        List<Claim> claims,
        TimeSpan? expiresIn = null,
        string? key = null,
        string? issuer = null,
        string? audience = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? DefaultKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer ?? DefaultIssuer,
            audience: audience ?? DefaultAudience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.Add(expiresIn ?? TimeSpan.FromHours(8)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate admin token for testing authentication
    /// </summary>
    public static string GenerateAdminToken(
        long adminId = 1,
        string username = "testadmin",
        Guid? tenantId = null,
        string? role = null,
        TimeSpan? expiresIn = null)
    {
        var claims = new List<Claim>
        {
            new("adminId", adminId.ToString()),
            new("username", username),
        };

        if (tenantId.HasValue)
            claims.Add(new Claim("tenantId", tenantId.Value.ToString()));

        if (!string.IsNullOrEmpty(role))
            claims.Add(new Claim("role", role));

        return GenerateToken(claims, expiresIn ?? TimeSpan.FromHours(8));
    }

    /// <summary>
    /// Generate resident/user token for testing
    /// </summary>
    public static string GenerateResidentToken(
        long residentId,
        Guid tenantId,
        string? phoneNumber = null,
        TimeSpan? expiresIn = null)
    {
        var claims = new List<Claim>
        {
            new("residentId", residentId.ToString()),
            new("tenantId", tenantId.ToString()),
            new("isResident", "true")
        };

        if (!string.IsNullOrEmpty(phoneNumber))
            claims.Add(new Claim("phoneNumber", phoneNumber));

        return GenerateToken(claims, expiresIn ?? TimeSpan.FromHours(8));
    }

    /// <summary>
    /// Generate expired token for testing token validation
    /// </summary>
    public static string GenerateExpiredToken(long adminId = 1)
    {
        return GenerateAdminToken(adminId, expiresIn: TimeSpan.FromSeconds(-1));
    }
}
