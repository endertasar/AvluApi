using System.Security.Claims;
using Moq;
using AvluApi.Models.Entities;
using AvluApi.Middleware;

namespace AvluApi.Tests.Fixtures;

/// <summary>
/// Fixture for Auth-related testing with JWT token generation and admin context
/// </summary>
public class AuthTestFixture : IDisposable
{
    private readonly Guid _tenantId;
    private readonly long _adminId;
    private readonly string _username;
    private readonly string _token;
    private readonly AdminUser _testAdmin;

    public Guid TenantId => _tenantId;
    public long AdminId => _adminId;
    public string Username => _username;
    public string Token => _token;
    public AdminUser TestAdmin => _testAdmin;

    public AuthTestFixture(
        Guid? tenantId = null,
        long adminId = 1,
        string? username = null)
    {
        _tenantId = tenantId ?? Guid.NewGuid();
        _adminId = adminId;
        _username = username ?? "testadmin";
        _testAdmin = TestDataBuilder.CreateAdminUser(
            adminId: adminId,
            username: _username);
        _token = TokenGenerationHelper.GenerateAdminToken(
            adminId: adminId,
            username: _username,
            tenantId: _tenantId);
    }

    /// <summary>
    /// Create a mock ITenantContext with this fixture's tenant/admin info
    /// </summary>
    public Mock<ITenantContext> CreateMockTenantContext(string? role = "Admin")
    {
        return MockFactory.CreateMockTenantContext(
            tenantId: _tenantId,
            adminId: _adminId,
            role: role ?? "Admin");
    }

    /// <summary>
    /// Create an admin token with additional custom claims
    /// </summary>
    public string CreateTokenWithCustomClaims(List<Claim> additionalClaims)
    {
        var baseClaims = new List<Claim>
        {
            new("adminId", _adminId.ToString()),
            new("username", _username),
            new("tenantId", _tenantId.ToString()),
        };

        baseClaims.AddRange(additionalClaims);
        return TokenGenerationHelper.GenerateToken(baseClaims);
    }

    /// <summary>
    /// Create an expired admin token for testing token validation
    /// </summary>
    public string CreateExpiredToken()
    {
        return TokenGenerationHelper.GenerateExpiredToken(_adminId);
    }

    /// <summary>
    /// Verify token validity (basic check)
    /// </summary>
    public bool IsTokenValid()
    {
        // In a real scenario, we'd validate the token signature and expiration
        // For now, check that it's not empty
        return !string.IsNullOrEmpty(_token);
    }

    public void Dispose()
    {
        // Cleanup if needed
        GC.SuppressFinalize(this);
    }
}
