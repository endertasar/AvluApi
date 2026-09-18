-- =========================================================
-- 0001_init.sql  (Avlu DB - Tam Şema)
-- =========================================================

CREATE TABLE dbo.__SchemaMigrations (
    ScriptName NVARCHAR(200) PRIMARY KEY,
    AppliedAt  DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

-- ---------- Tenant (Site) ----------
CREATE TABLE dbo.Sites (
    Id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteUsername  NVARCHAR(50)  NOT NULL,
    Name          NVARCHAR(200) NOT NULL,
    Address       NVARCHAR(500) NULL,
    CreatedAt     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted     BIT           NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Sites_SiteUsername UNIQUE (SiteUsername)
);

-- ---------- Yönetici kullanıcılar ----------
CREATE TABLE dbo.AdminUsers (
    Id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId        BIGINT        NOT NULL,
    Username      NVARCHAR(50)  NOT NULL,
    PasswordHash  NVARCHAR(255) NOT NULL,
    FullName      NVARCHAR(200) NULL,
    Role          NVARCHAR(20)  NOT NULL DEFAULT 'SubUser',
    IsActive      BIT           NOT NULL DEFAULT 1,
    CreatedAt     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted     BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_AdminUsers_Sites FOREIGN KEY (SiteId) REFERENCES dbo.Sites(Id),
    CONSTRAINT UQ_AdminUsers_Site_Username UNIQUE (SiteId, Username)
);

-- ---------- Site sakinleri ----------
CREATE TABLE dbo.Residents (
    Id               BIGINT IDENTITY(1,1) PRIMARY KEY,
    Phone            NVARCHAR(20)  NOT NULL,
    PasswordHash     NVARCHAR(255) NOT NULL,
    FullName         NVARCHAR(200) NULL,
    IsPhoneVerified  BIT           NOT NULL DEFAULT 1,
    OneSignalUserId  NVARCHAR(100) NULL,
    CreatedAt        DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted        BIT           NOT NULL DEFAULT 0,
    CONSTRAINT UQ_Residents_Phone UNIQUE (Phone)
);

-- ---------- Mülkler ----------
CREATE TABLE dbo.Properties (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId          BIGINT        NOT NULL,
    Block           NVARCHAR(50)  NULL,
    UnitNo          NVARCHAR(50)  NOT NULL,
    Type            NVARCHAR(20)  NOT NULL DEFAULT 'Daire',
    OwnerName       NVARCHAR(200) NULL,
    OwnerPhone      NVARCHAR(20)  NULL,
    ResidentName    NVARCHAR(200) NULL,
    ResidentPhone   NVARCHAR(20)  NULL,
    DuesResponsible NVARCHAR(20)  NOT NULL DEFAULT 'Owner',
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted       BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_Properties_Sites FOREIGN KEY (SiteId) REFERENCES dbo.Sites(Id)
);
CREATE INDEX IX_Properties_SiteId        ON dbo.Properties(SiteId);
CREATE INDEX IX_Properties_OwnerPhone    ON dbo.Properties(OwnerPhone);
CREATE INDEX IX_Properties_ResidentPhone ON dbo.Properties(ResidentPhone);

-- ---------- Aidat tanımı ----------
CREATE TABLE dbo.DuesDefinitions (
    Id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId        BIGINT        NOT NULL,
    PropertyType  NVARCHAR(20)  NOT NULL,
    Amount        DECIMAL(18,2) NOT NULL,
    EffectiveFrom INT           NOT NULL,
    Description   NVARCHAR(200) NULL,
    CreatedAt     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted     BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_DuesDefinitions_Sites FOREIGN KEY (SiteId) REFERENCES dbo.Sites(Id)
);
CREATE INDEX IX_DuesDefinitions_Site_Type ON dbo.DuesDefinitions(SiteId, PropertyType, EffectiveFrom);

-- ---------- Aidat tahakkukları ----------
CREATE TABLE dbo.DuesCharges (
    Id          BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId      BIGINT        NOT NULL,
    PropertyId  BIGINT        NOT NULL,
    Period      INT           NOT NULL,
    Amount      DECIMAL(18,2) NOT NULL,
    PaidAmount  DECIMAL(18,2) NOT NULL DEFAULT 0,
    DueDate     DATETIME2     NULL,
    Status      NVARCHAR(20)  NOT NULL DEFAULT 'Pending',
    CreatedAt   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted   BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_DuesCharges_Sites      FOREIGN KEY (SiteId)     REFERENCES dbo.Sites(Id),
    CONSTRAINT FK_DuesCharges_Properties FOREIGN KEY (PropertyId) REFERENCES dbo.Properties(Id),
    CONSTRAINT UQ_DuesCharges_Property_Period UNIQUE (PropertyId, Period)
);
CREATE INDEX IX_DuesCharges_Property ON dbo.DuesCharges(PropertyId);
CREATE INDEX IX_DuesCharges_Status   ON dbo.DuesCharges(SiteId, Status);

-- ---------- Ek ödeme tanımı ----------
CREATE TABLE dbo.ExtraPayments (
    Id                BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId            BIGINT        NOT NULL,
    Name              NVARCHAR(200) NOT NULL,
    AmountPerProperty DECIMAL(18,2) NOT NULL,
    TotalAmount       DECIMAL(18,2) NULL,
    InstallmentCount  INT           NOT NULL DEFAULT 1,
    CreatedAt         DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted         BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_ExtraPayments_Sites FOREIGN KEY (SiteId) REFERENCES dbo.Sites(Id)
);

-- ---------- Ek ödeme tahakkukları ----------
CREATE TABLE dbo.ExtraPaymentCharges (
    Id             BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId         BIGINT        NOT NULL,
    ExtraPaymentId BIGINT        NOT NULL,
    PropertyId     BIGINT        NOT NULL,
    InstallmentNo  INT           NOT NULL DEFAULT 1,
    Amount         DECIMAL(18,2) NOT NULL,
    PaidAmount     DECIMAL(18,2) NOT NULL DEFAULT 0,
    DueDate        DATETIME2     NULL,
    Status         NVARCHAR(20)  NOT NULL DEFAULT 'Pending',
    CreatedAt      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted      BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_EPC_Sites         FOREIGN KEY (SiteId)         REFERENCES dbo.Sites(Id),
    CONSTRAINT FK_EPC_ExtraPayments FOREIGN KEY (ExtraPaymentId) REFERENCES dbo.ExtraPayments(Id),
    CONSTRAINT FK_EPC_Properties    FOREIGN KEY (PropertyId)     REFERENCES dbo.Properties(Id)
);
CREATE INDEX IX_EPC_Property ON dbo.ExtraPaymentCharges(PropertyId);

-- ---------- Tahsilatlar ----------
CREATE TABLE dbo.Payments (
    Id                BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId            BIGINT        NOT NULL,
    PropertyId        BIGINT        NOT NULL,
    TargetType        NVARCHAR(20)  NOT NULL,
    TargetId          BIGINT        NOT NULL,
    Amount            DECIMAL(18,2) NOT NULL,
    Method            NVARCHAR(20)  NOT NULL DEFAULT 'Cash',
    Note              NVARCHAR(500) NULL,
    PaidAt            DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CollectedByUserId BIGINT        NULL,
    IsDeleted         BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_Payments_Sites      FOREIGN KEY (SiteId)     REFERENCES dbo.Sites(Id),
    CONSTRAINT FK_Payments_Properties FOREIGN KEY (PropertyId) REFERENCES dbo.Properties(Id)
);
CREATE INDEX IX_Payments_Target   ON dbo.Payments(TargetType, TargetId);
CREATE INDEX IX_Payments_Property ON dbo.Payments(PropertyId);

-- ---------- Mülk alacak bakiyesi ----------
CREATE TABLE dbo.PropertyBalances (
    Id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId        BIGINT        NOT NULL,
    PropertyId    BIGINT        NOT NULL,
    CreditBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
    UpdatedAt     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PropertyBalances_Sites      FOREIGN KEY (SiteId)     REFERENCES dbo.Sites(Id),
    CONSTRAINT FK_PropertyBalances_Properties FOREIGN KEY (PropertyId) REFERENCES dbo.Properties(Id),
    CONSTRAINT UQ_PropertyBalances_Property UNIQUE (PropertyId)
);

-- ---------- Bakiye hareketleri ----------
CREATE TABLE dbo.BalanceTransactions (
    Id         BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId     BIGINT        NOT NULL,
    PropertyId BIGINT        NOT NULL,
    Direction  NVARCHAR(10)  NOT NULL,
    Amount     DECIMAL(18,2) NOT NULL,
    Source     NVARCHAR(20)  NOT NULL,
    SourceId   BIGINT        NULL,
    Note       NVARCHAR(500) NULL,
    CreatedAt  DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_BalanceTx_Sites      FOREIGN KEY (SiteId)     REFERENCES dbo.Sites(Id),
    CONSTRAINT FK_BalanceTx_Properties FOREIGN KEY (PropertyId) REFERENCES dbo.Properties(Id)
);
CREATE INDEX IX_BalanceTx_Property ON dbo.BalanceTransactions(PropertyId, CreatedAt);

-- ---------- Giderler ----------
CREATE TABLE dbo.Expenses (
    Id              BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId          BIGINT        NOT NULL,
    Category        NVARCHAR(100) NULL,
    Amount          DECIMAL(18,2) NOT NULL,
    ExpenseDate     DATETIME2     NOT NULL,
    Description     NVARCHAR(500) NULL,
    CreatedByUserId BIGINT        NULL,
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted       BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_Expenses_Sites FOREIGN KEY (SiteId) REFERENCES dbo.Sites(Id)
);
CREATE INDEX IX_Expenses_Site_Date ON dbo.Expenses(SiteId, ExpenseDate);

-- ---------- Bildirimler ----------
CREATE TABLE dbo.Notifications (
    Id            BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId        BIGINT        NOT NULL,
    Title         NVARCHAR(200) NOT NULL,
    Body          NVARCHAR(1000) NOT NULL,
    TargetType    NVARCHAR(20)  NOT NULL DEFAULT 'All',
    TargetId      BIGINT        NULL,
    SentByUserId  BIGINT        NULL,
    SentAt        DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    IsDeleted     BIT           NOT NULL DEFAULT 0,
    CONSTRAINT FK_Notifications_Sites FOREIGN KEY (SiteId) REFERENCES dbo.Sites(Id)
);
CREATE INDEX IX_Notifications_Site ON dbo.Notifications(SiteId, SentAt);

-- ---------- Bildirim okundu takibi ----------
CREATE TABLE dbo.NotificationReads (
    Id             BIGINT IDENTITY(1,1) PRIMARY KEY,
    NotificationId BIGINT    NOT NULL,
    ResidentId     BIGINT    NOT NULL,
    ReadAt         DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_NReads_Notifications FOREIGN KEY (NotificationId) REFERENCES dbo.Notifications(Id),
    CONSTRAINT FK_NReads_Residents     FOREIGN KEY (ResidentId)     REFERENCES dbo.Residents(Id),
    CONSTRAINT UQ_NReads UNIQUE (NotificationId, ResidentId)
);

-- ---------- Refresh token'lar ----------
CREATE TABLE dbo.RefreshTokens (
    Id         BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserType   NVARCHAR(20)  NOT NULL,
    UserId     BIGINT        NOT NULL,
    Token      NVARCHAR(255) NOT NULL,
    ExpiresAt  DATETIME2     NOT NULL,
    RevokedAt  DATETIME2     NULL,
    CreatedAt  DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_RefreshTokens_Token UNIQUE (Token)
);
CREATE INDEX IX_RefreshTokens_User ON dbo.RefreshTokens(UserType, UserId);

-- ---------- Denetim kaydı ----------
CREATE TABLE dbo.AuditLogs (
    Id        BIGINT IDENTITY(1,1) PRIMARY KEY,
    SiteId    BIGINT        NULL,
    UserType  NVARCHAR(20)  NULL,
    UserId    BIGINT        NULL,
    Action    NVARCHAR(50)  NOT NULL,
    Entity    NVARCHAR(50)  NOT NULL,
    EntityId  BIGINT        NULL,
    Detail    NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME()
);
CREATE INDEX IX_AuditLogs_Site_Date ON dbo.AuditLogs(SiteId, CreatedAt);
