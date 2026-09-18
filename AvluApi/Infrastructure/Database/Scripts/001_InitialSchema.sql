-- ============================================================
-- 001_InitialSchema.sql  –  AvluApi Initial Database Schema
-- ============================================================

-- -----------------------------------------------
-- GLOBAL TABLOLAR (TenantId yok)
-- -----------------------------------------------

CREATE TABLE Tenants (
    TenantId     UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TenantName   NVARCHAR(200) NOT NULL,
    TenantCode   NVARCHAR(50)  NOT NULL UNIQUE,
    Address      NVARCHAR(500),
    IsActive     BIT NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE AdminUsers (
    AdminId       BIGINT IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(256) NOT NULL,
    Email         NVARCHAR(200),
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE AdminTenantMap (
    AdminId     BIGINT NOT NULL REFERENCES AdminUsers(AdminId),
    TenantId    UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(TenantId),
    Role        NVARCHAR(50) NOT NULL DEFAULT 'Manager', -- SuperAdmin | Manager
    AssignedAt  DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PRIMARY KEY (AdminId, TenantId)
);

CREATE TABLE PhonePropertyMap (
    MapId        BIGINT IDENTITY(1,1) PRIMARY KEY,
    PhoneNumber  NVARCHAR(20) NOT NULL,
    PropertyId   BIGINT NOT NULL,
    TenantId     UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(TenantId),
    IsActive     BIT NOT NULL DEFAULT 1,
    AssignedAt   DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE DeviceTokens (
    TokenId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    PhoneNumber  NVARCHAR(20) NOT NULL,
    FCMToken     NVARCHAR(500) NOT NULL,
    Platform     NVARCHAR(20) NOT NULL, -- Android | iOS
    UpdatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

-- -----------------------------------------------
-- TENANT TABLOLARI (her birinde TenantId var)
-- -----------------------------------------------

CREATE TABLE Properties (
    PropertyId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId      UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(TenantId),
    BlockName     NVARCHAR(50),
    DoorNumber    NVARCHAR(20) NOT NULL,
    Floor         INT,
    PropertyType  NVARCHAR(50) NOT NULL DEFAULT 'Apartment', -- Apartment | Commercial | Other
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Residents (
    ResidentId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId      UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(TenantId),
    PropertyId    BIGINT NOT NULL REFERENCES Properties(PropertyId),
    FullName      NVARCHAR(200) NOT NULL,
    PhoneNumber   NVARCHAR(20) NOT NULL,
    ResidentType  NVARCHAR(20) NOT NULL DEFAULT 'Owner', -- Owner | Tenant
    MoveInDate    DATE,
    MoveOutDate   DATE,
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE DuesDefinitions (
    DuesDefId    BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId     UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(TenantId),
    Name         NVARCHAR(100) NOT NULL,
    Amount       DECIMAL(18,2) NOT NULL,
    DueDay       INT NOT NULL DEFAULT 1, -- ayın kaçında vadesi dolacak (1-31)
    PeriodType   NVARCHAR(20) NOT NULL DEFAULT 'Monthly', -- Monthly | OneTime
    IsActive     BIT NOT NULL DEFAULT 1,
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE Charges (
    ChargeId     BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId     UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(TenantId),
    PropertyId   BIGINT NOT NULL REFERENCES Properties(PropertyId),
    DuesDefId    BIGINT REFERENCES DuesDefinitions(DuesDefId),
    Description  NVARCHAR(200),
    Amount       DECIMAL(18,2) NOT NULL,
    PaidAmount   DECIMAL(18,2) NOT NULL DEFAULT 0,
    DueDate      DATE NOT NULL,
    PeriodYear   INT,
    PeriodMonth  INT,
    Status       NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- Pending | Partial | Paid
    CreatedAt    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_Charge_Period UNIQUE (TenantId, PropertyId, DuesDefId, PeriodYear, PeriodMonth)
);

CREATE TABLE Payments (
    PaymentId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId       UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(TenantId),
    ChargeId       BIGINT NOT NULL REFERENCES Charges(ChargeId),
    PropertyId     BIGINT NOT NULL REFERENCES Properties(PropertyId),
    PaidAmount     DECIMAL(18,2) NOT NULL,
    PaymentDate    DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PaymentMethod  NVARCHAR(20) NOT NULL DEFAULT 'Cash', -- Cash | Transfer | Card
    ReceiptNumber  NVARCHAR(50),
    CollectedBy    BIGINT REFERENCES AdminUsers(AdminId),
    Notes          NVARCHAR(500)
);

CREATE TABLE Notifications (
    NotifId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId     UNIQUEIDENTIFIER NOT NULL REFERENCES Tenants(TenantId),
    Title        NVARCHAR(200) NOT NULL,
    Body         NVARCHAR(MAX) NOT NULL,
    TargetType   NVARCHAR(20) NOT NULL DEFAULT 'All', -- All | Block | Property | Resident
    TargetId     NVARCHAR(100),
    SentAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    SentBy       BIGINT REFERENCES AdminUsers(AdminId)
);
