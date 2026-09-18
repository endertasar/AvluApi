using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AvluApi.Infrastructure;
using AvluApi.Infrastructure.Database;
using AvluApi.Models.DTOs;
using AvluApi.Models.Entities;
using AvluApi.Models.Requests;
using AvluApi.Services.Interfaces;
using Dapper;
using Microsoft.IdentityModel.Tokens;

namespace AvluApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IDbConnectionFactory _db;
    private readonly IConfiguration _config;

    public AuthService(IDbConnectionFactory db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AdminRegisterResponse> RegisterAdminAsync(AdminRegisterRequest req)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            var siteExists = await conn.QueryFirstOrDefaultAsync<int>(
                "SELECT 1 FROM Sites WHERE SiteUsername = @SiteUsername AND IsDeleted = 0",
                new { req.SiteUsername }, tx);
            if (siteExists == 1)
                throw new ConflictException("Bu site kullanıcı adı zaten kayıtlı.");

            var siteId = await conn.QuerySingleAsync<long>(
                "INSERT INTO Sites (SiteUsername, Name, Address) OUTPUT INSERTED.Id VALUES (@SiteUsername, @Name, @Address);",
                new { req.SiteUsername, Name = req.SiteName, req.Address }, tx);

            var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
            var adminId = await conn.QuerySingleAsync<long>(
                @"INSERT INTO AdminUsers (SiteId, Username, PasswordHash, FullName, Role)
                  OUTPUT INSERTED.Id
                  VALUES (@SiteId, @Username, @PasswordHash, @FullName, 'Owner');",
                new { SiteId = siteId, req.Username, PasswordHash = hash, req.FullName }, tx);

            tx.Commit();

            var (access, refresh) = await IssueTokenPairAsync(conn, "admin", adminId, siteId, "Owner");
            return new AdminRegisterResponse(
                new SiteDto(siteId, req.SiteUsername, req.SiteName),
                access,
                refresh);
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<AdminLoginResponse> LoginAdminAsync(AdminLoginRequest req)
    {
        using var conn = _db.CreateConnection();

        var site = await conn.QueryFirstOrDefaultAsync<Site>(
            "SELECT * FROM Sites WHERE SiteUsername = @SiteUsername AND IsDeleted = 0",
            new { req.SiteUsername })
            ?? throw new UnauthorizedException("Site kodu veya kimlik bilgileri hatalı.");

        var admin = await conn.QueryFirstOrDefaultAsync<AdminUser>(
            "SELECT * FROM AdminUsers WHERE SiteId = @SiteId AND Username = @Username AND IsDeleted = 0 AND IsActive = 1",
            new { SiteId = site.Id, req.Username })
            ?? throw new UnauthorizedException("Site kodu veya kimlik bilgileri hatalı.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, admin.PasswordHash))
            throw new UnauthorizedException("Site kodu veya kimlik bilgileri hatalı.");

        var (access, refresh) = await IssueTokenPairAsync(conn, "admin", admin.Id, site.Id, admin.Role);

        return new AdminLoginResponse(
            access,
            refresh,
            new AdminUserInfo(admin.Id, admin.Username, admin.FullName, admin.Role, admin.IsActive));
    }

    public async Task<ResidentLoginResponse> RegisterResidentAsync(ResidentRegisterRequest req)
    {
        using var conn = _db.CreateConnection();

        var exists = await conn.QueryFirstOrDefaultAsync<int>(
            "SELECT 1 FROM Residents WHERE Phone = @Phone AND IsDeleted = 0",
            new { req.Phone });
        if (exists == 1)
            throw new ConflictException("Bu telefon numarası zaten kayıtlı.");

        var hash = BCrypt.Net.BCrypt.HashPassword(req.Password);
        var residentId = await conn.QuerySingleAsync<long>(
            "INSERT INTO Residents (Phone, PasswordHash, FullName) OUTPUT INSERTED.Id VALUES (@Phone, @PasswordHash, @FullName);",
            new { req.Phone, PasswordHash = hash, req.FullName });

        var (access, refresh) = await IssueTokenPairAsync(conn, "resident", residentId, 0, string.Empty);

        return new ResidentLoginResponse(
            access,
            refresh,
            new ResidentInfo(residentId, req.Phone, req.FullName));
    }

    public async Task<ResidentLoginResponse> LoginResidentAsync(ResidentLoginRequest req)
    {
        using var conn = _db.CreateConnection();

        var resident = await conn.QueryFirstOrDefaultAsync<Resident>(
            "SELECT * FROM Residents WHERE Phone = @Phone AND IsDeleted = 0",
            new { req.Phone })
            ?? throw new UnauthorizedException("Telefon numarası veya şifre hatalı.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, resident.PasswordHash))
            throw new UnauthorizedException("Telefon numarası veya şifre hatalı.");

        var (access, refresh) = await IssueTokenPairAsync(conn, "resident", resident.Id, 0, string.Empty);

        return new ResidentLoginResponse(
            access,
            refresh,
            new ResidentInfo(resident.Id, resident.Phone, resident.FullName));
    }

    public async Task<TokenRefreshResponse> RefreshTokenAsync(string refreshToken)
    {
        using var conn = _db.CreateConnection();

        var token = await conn.QueryFirstOrDefaultAsync<RefreshToken>(
            "SELECT * FROM RefreshTokens WHERE Token = @Token",
            new { Token = refreshToken })
            ?? throw new UnauthorizedException("Geçersiz refresh token.");

        if (!token.IsActive)
            throw new UnauthorizedException("Refresh token süresi dolmuş veya iptal edilmiş.");

        await conn.ExecuteAsync(
            "UPDATE RefreshTokens SET RevokedAt = SYSUTCDATETIME() WHERE Id = @Id",
            new { token.Id });

        string role = string.Empty;
        long siteId = 0;

        if (token.UserType == "admin")
        {
            var admin = await conn.QueryFirstOrDefaultAsync<AdminUser>(
                "SELECT * FROM AdminUsers WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1",
                new { Id = token.UserId })
                ?? throw new UnauthorizedException("Kullanıcı bulunamadı.");
            role = admin.Role;
            siteId = admin.SiteId;
        }

        var (access, newRefresh) = await IssueTokenPairAsync(conn, token.UserType, token.UserId, siteId, role);
        return new TokenRefreshResponse(access, newRefresh);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        using var conn = _db.CreateConnection();
        await conn.ExecuteAsync(
            "UPDATE RefreshTokens SET RevokedAt = SYSUTCDATETIME() WHERE Token = @Token AND RevokedAt IS NULL",
            new { Token = refreshToken });
    }

    // ── Yardımcı metodlar ────────────────────────────────────────────────────

    private async Task<(string Access, string Refresh)> IssueTokenPairAsync(
        System.Data.IDbConnection conn, string userType, long userId, long siteId, string role)
    {
        var access = GenerateAccessToken(userType, userId, siteId, role);
        var refresh = await CreateRefreshTokenAsync(conn, userType, userId);
        return (access, refresh);
    }

    private string GenerateAccessToken(string userType, long userId, long siteId, string role)
    {
        var jwtSection = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("sub", userId.ToString()),
            new("user_type", userType),
        };

        if (userType == "admin")
        {
            claims.Add(new("site_id", siteId.ToString()));
            claims.Add(new("role", role));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static async Task<string> CreateRefreshTokenAsync(
        System.Data.IDbConnection conn, string userType, long userId)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        await conn.ExecuteAsync(
            "INSERT INTO RefreshTokens (UserType, UserId, Token, ExpiresAt) VALUES (@UserType, @UserId, @Token, @ExpiresAt);",
            new { UserType = userType, UserId = userId, Token = token, ExpiresAt = DateTime.UtcNow.AddDays(30) });
        return token;
    }
}
