using Moq;
using AvluApi.Models.Entities;
using AvluApi.Middleware;

namespace AvluApi.Tests.Fixtures;

/// <summary>
/// Fixture for testing multi-tenant scenarios and tenant isolation
/// </summary>
public class TenantTestFixture : IDisposable
{
    private readonly Guid _tenantId;
    private readonly Tenant _testTenant;
    private readonly List<Property> _testProperties;
    private readonly List<Resident> _testResidents;

    public Guid TenantId => _tenantId;
    public Tenant TestTenant => _testTenant;
    public List<Property> TestProperties => _testProperties;
    public List<Resident> TestResidents => _testResidents;

    public TenantTestFixture(Guid? tenantId = null, int propertyCount = 3, int residentsPerProperty = 5)
    {
        _tenantId = tenantId ?? Guid.NewGuid();
        _testTenant = TestDataBuilder.CreateTenant(tenantId: _tenantId);
        
        _testProperties = TestDataBuilder.CreateProperties(propertyCount, tenantId: _tenantId);
        
        _testResidents = new List<Resident>();
        foreach (var property in _testProperties)
        {
            for (int i = 0; i < residentsPerProperty; i++)
            {
                var resident = TestDataBuilder.CreateResident(propertyId: property.PropertyId);
                _testResidents.Add(resident);
            }
        }
    }

    /// <summary>
    /// Create a mock ITenantContext with this fixture's tenant
    /// </summary>
    public Mock<ITenantContext> CreateMockTenantContext(long adminId = 1)
    {
        return MockFactory.CreateMockTenantContext(
            tenantId: _tenantId,
            adminId: adminId,
            role: "Admin");
    }

    /// <summary>
    /// Create a resident token for this tenant
    /// </summary>
    public string CreateResidentToken(long? residentId = null, string? phoneNumber = null)
    {
        var rId = residentId ?? _testResidents.FirstOrDefault()?.ResidentId ?? 1;
        return TokenGenerationHelper.GenerateResidentToken(
            residentId: rId,
            tenantId: _tenantId,
            phoneNumber: phoneNumber);
    }

    /// <summary>
    /// Get a random property from this tenant
    /// </summary>
    public Property GetRandomProperty()
    {
        if (_testProperties.Count == 0)
            throw new InvalidOperationException("No properties available in this tenant fixture.");
        
        var randomIndex = new Random().Next(_testProperties.Count);
        return _testProperties[randomIndex];
    }

    /// <summary>
    /// Get a random resident from this tenant
    /// </summary>
    public Resident GetRandomResident()
    {
        if (_testResidents.Count == 0)
            throw new InvalidOperationException("No residents available in this tenant fixture.");
        
        var randomIndex = new Random().Next(_testResidents.Count);
        return _testResidents[randomIndex];
    }

    /// <summary>
    /// Verify tenant isolation: ensure different tenant IDs
    /// </summary>
    public bool VerifyTenantIsolation(Guid anotherTenantId)
    {
        return _tenantId != anotherTenantId;
    }

    public void Dispose()
    {
        _testProperties.Clear();
        _testResidents.Clear();
        GC.SuppressFinalize(this);
    }
}
