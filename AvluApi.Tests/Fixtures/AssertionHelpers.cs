using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;

namespace AvluApi.Tests.Fixtures;

/// <summary>
/// Helper methods for common test assertions and utilities
/// </summary>
public static class AssertionHelpers
{
    /// <summary>
    /// Validate JWT token structure and claims
    /// </summary>
    public static void ValidateToken(string token, List<string>? expectedClaims = null)
    {
        token.Should().NotBeNullOrEmpty("Token should be generated");

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            jwtToken.Should().NotBeNull("Token should be valid JWT");
            jwtToken.Issuer.Should().Be("AvluApi", "Token issuer should be AvluApi");
            jwtToken.Audiences.Should().Contain("AvluApiClients", "Token audience should be AvluApiClients");
            jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow, "Token should not be expired");

            if (expectedClaims != null && expectedClaims.Count > 0)
            {
                foreach (var claimType in expectedClaims)
                {
                    jwtToken.Claims.Any(c => c.Type == claimType)
                        .Should().BeTrue($"Token should contain claim '{claimType}'");
                }
            }
        }
        catch (Exception ex)
        {
            throw new AssertionFailedException($"Invalid token: {ex.Message}");
        }
    }

    /// <summary>
    /// Validate that token is expired
    /// </summary>
    public static void ValidateExpiredToken(string token)
    {
        token.Should().NotBeNullOrEmpty("Token should be generated");

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow, "Token should be expired");
    }

    /// <summary>
    /// Extract claim value from token
    /// </summary>
    public static string? ExtractClaimValue(string token, string claimType)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    /// <summary>
    /// Basic response validation helper
    /// </summary>
    public static void ValidateSuccessResponse<T>(T? response, string? failureMessage = null)
    {
        response.Should().NotBeNull(failureMessage ?? "Response should not be null");
    }

    /// <summary>
    /// Validate pagination properties
    /// </summary>
    public static void ValidatePaginationData(int pageNumber, int pageSize, int totalCount, int itemCount)
    {
        pageNumber.Should().BeGreaterThan(0, "Page number should be greater than 0");
        pageSize.Should().BeGreaterThan(0, "Page size should be greater than 0");
        totalCount.Should().BeGreaterThanOrEqualTo(0, "Total count should not be negative");
        itemCount.Should().BeLessThanOrEqualTo(pageSize, "Item count should not exceed page size");
    }

    /// <summary>
    /// Validate tenant isolation for multiple entities
    /// </summary>
    public static void ValidateTenantIsolation(Guid expectedTenantId, params Guid[] actualTenantIds)
    {
        foreach (var tenantId in actualTenantIds)
        {
            tenantId.Should().Be(expectedTenantId, "Tenant IDs should match and enforce isolation");
        }
    }
}
