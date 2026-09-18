using Moq;
using AvluApi.Middleware;
using AvluApi.Repositories.Interfaces;
using AvluApi.Services.Interfaces;
using System.Data;

namespace AvluApi.Tests.Fixtures;

/// <summary>
/// Factory for creating Moq mocks of repositories, services, and infrastructure components
/// </summary>
public static class MockFactory
{
    /// <summary>
    /// Create a mock ITenantContext with default values
    /// </summary>
    public static Mock<ITenantContext> CreateMockTenantContext(
        Guid? tenantId = null,
        long adminId = 1,
        string role = "Admin",
        string? phoneNumber = null,
        bool isResidentToken = false)
    {
        var mock = new Mock<ITenantContext>();
        
        mock.Setup(x => x.TenantId).Returns(tenantId ?? Guid.NewGuid());
        mock.Setup(x => x.AdminId).Returns(adminId);
        mock.Setup(x => x.Role).Returns(role);
        mock.Setup(x => x.PhoneNumber).Returns(phoneNumber);
        mock.Setup(x => x.IsResidentToken).Returns(isResidentToken);

        return mock;
    }

    /// <summary>
    /// Create a mock IDbConnection
    /// </summary>
    public static Mock<IDbConnection> CreateMockDbConnection()
    {
        var mock = new Mock<IDbConnection>();
        mock.Setup(x => x.State).Returns(ConnectionState.Open);
        return mock;
    }

    /// <summary>
    /// Create a mock IDbConnectionFactory
    /// </summary>
    public static Mock<IDbConnectionFactory> CreateMockDbConnectionFactory()
    {
        var mock = new Mock<IDbConnectionFactory>();
        var dbConnectionMock = CreateMockDbConnection();
        
        mock.Setup(x => x.CreateConnection())
            .Returns(dbConnectionMock.Object);
        
        return mock;
    }

    /// <summary>
    /// Create a mock IAdminRepository
    /// </summary>
    public static Mock<IAdminRepository> CreateMockAdminRepository()
    {
        return new Mock<IAdminRepository>();
    }

    /// <summary>
    /// Create a mock IPhoneMapRepository
    /// </summary>
    public static Mock<IPhoneMapRepository> CreateMockPhoneMapRepository()
    {
        return new Mock<IPhoneMapRepository>();
    }

    /// <summary>
    /// Create a mock IPropertyRepository
    /// </summary>
    public static Mock<IPropertyRepository> CreateMockPropertyRepository()
    {
        return new Mock<IPropertyRepository>();
    }

    /// <summary>
    /// Create a mock IResidentRepository
    /// </summary>
    public static Mock<IResidentRepository> CreateMockResidentRepository()
    {
        return new Mock<IResidentRepository>();
    }

    /// <summary>
    /// Create a mock ITenantRepository
    /// </summary>
    public static Mock<ITenantRepository> CreateMockTenantRepository()
    {
        return new Mock<ITenantRepository>();
    }

    /// <summary>
    /// Create a mock IDuesRepository
    /// </summary>
    public static Mock<IDuesRepository> CreateMockDuesRepository()
    {
        return new Mock<IDuesRepository>();
    }

    /// <summary>
    /// Create a mock IChargeRepository
    /// </summary>
    public static Mock<IChargeRepository> CreateMockChargeRepository()
    {
        return new Mock<IChargeRepository>();
    }

    /// <summary>
    /// Create a mock IPaymentRepository
    /// </summary>
    public static Mock<IPaymentRepository> CreateMockPaymentRepository()
    {
        return new Mock<IPaymentRepository>();
    }

    /// <summary>
    /// Create a mock INotificationRepository
    /// </summary>
    public static Mock<INotificationRepository> CreateMockNotificationRepository()
    {
        return new Mock<INotificationRepository>();
    }

    /// <summary>
    /// Create a mock IAuthService
    /// </summary>
    public static Mock<IAuthService> CreateMockAuthService()
    {
        return new Mock<IAuthService>();
    }

    /// <summary>
    /// Create a mock ITenantService
    /// </summary>
    public static Mock<ITenantService> CreateMockTenantService()
    {
        return new Mock<ITenantService>();
    }

    /// <summary>
    /// Create a mock IPropertyService
    /// </summary>
    public static Mock<IPropertyService> CreateMockPropertyService()
    {
        return new Mock<IPropertyService>();
    }

    /// <summary>
    /// Create a mock IResidentService
    /// </summary>
    public static Mock<IResidentService> CreateMockResidentService()
    {
        return new Mock<IResidentService>();
    }

    /// <summary>
    /// Create a mock IDuesService
    /// </summary>
    public static Mock<IDuesService> CreateMockDuesService()
    {
        return new Mock<IDuesService>();
    }

    /// <summary>
    /// Create a mock IChargeService
    /// </summary>
    public static Mock<IChargeService> CreateMockChargeService()
    {
        return new Mock<IChargeService>();
    }

    /// <summary>
    /// Create a mock IPaymentService
    /// </summary>
    public static Mock<IPaymentService> CreateMockPaymentService()
    {
        return new Mock<IPaymentService>();
    }

    /// <summary>
    /// Create a mock INotificationService
    /// </summary>
    public static Mock<INotificationService> CreateMockNotificationService()
    {
        return new Mock<INotificationService>();
    }

    /// <summary>
    /// Create a mock IReportService
    /// </summary>
    public static Mock<IReportService> CreateMockReportService()
    {
        return new Mock<IReportService>();
    }

    /// <summary>
    /// Create a mock IFcmService
    /// </summary>
    public static Mock<IFcmService> CreateMockFcmService()
    {
        return new Mock<IFcmService>();
    }
}
