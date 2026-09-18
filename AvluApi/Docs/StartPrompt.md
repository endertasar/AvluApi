AvluApi adında bir ASP.NET Core Web API projesi oluşturdum. 
Bu proje çok kiracılı (multi-tenant) bir apartman/site yönetim sisteminin backend'i olacak.
Aşağıdaki gereksinimlere göre projeyi yapılandır.

---

## GENEL MİMARİ

- .NET 9 Web API
- Veritabanı: SQL Server
- ORM: Dapper (Entity Framework kullanma)
- Kimlik doğrulama: JWT Bearer
- Multi-tenant yapı: Shared Database, Shared Schema — her tabloda TenantId (Guid) kolonu var
- Her API isteğinde TenantId, JWT claim'inden çözümlenir ve tüm sorgulara otomatik uygulanır

---

## PROJE KLASÖR YAPISI

Aşağıdaki klasör yapısını oluştur:

AvluApi/
├── Controllers/
├── Middleware/
├── Models/
│   ├── Entities/
│   ├── DTOs/
│   └── Requests/
├── Repositories/
│   ├── Interfaces/
│   └── Implementations/
├── Services/
│   ├── Interfaces/
│   └── Implementations/
├── Infrastructure/
│   ├── Database/        # Dapper bağlantısı, DbContext yerine IDbConnectionFactory
│   └── Extensions/      # ServiceCollection extension'ları
└── BackgroundJobs/

---

## VERİTABANI TABLOLARI

Aşağıdaki SQL Server tablolarını oluşturacak migration script dosyasını 
Infrastructure/Database/Scripts/001_InitialSchema.sql olarak oluştur.

### GLOBAL TABLOLAR (TenantId yok)

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

### TENANT TABLOLARI (her birinde TenantId var)

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
  TargetId     NVARCHAR(100), -- Block adı, PropertyId veya PhoneNumber
  SentAt       DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
  SentBy       BIGINT REFERENCES AdminUsers(AdminId)
);

---

## KATMANLAR VE KURALLAR

### IDbConnectionFactory
Infrastructure/Database/ altında IDbConnectionFactory interface'i ve 
SqlConnectionFactory implementasyonunu oluştur.
appsettings.json'daki "DefaultConnection" connection string'ini kullanacak.

### TenantContext
Middleware/TenantContext.cs — JWT claim'inden TenantId ve AdminId'yi çözen,
IHttpContextAccessor üzerinden her katmana inject edilebilen bir servis.
Claim key'leri: "tenantId", "adminId", "role"

### TenantMiddleware
Her istekte Authorization header'ından JWT doğrula.
/api/auth/* endpoint'leri bu middleware'den muaf olacak.
TenantId claim'i yoksa 403 döndür.

### Base Repository
Repositories/ altında tüm tenant repository'lerin kalıtacağı BaseRepository<T> sınıfı.
Constructor'da ITenantContext inject edilir.
TenantId tüm sorgulara otomatik eklenir.
Hiçbir public metot TenantId parametresi almaz — context'ten otomatik gelir.

---

## CONTROLLER VE SERVİSLER

Aşağıdaki controller/service/repository üçlülerini oluştur.
Her biri için interface + implementation yap.

### 1. AuthController  /api/auth
- POST /login          → AdminUser için username+password, JWT döner (claim: adminId, tenantId listesi, role)
- POST /tenant-select  → Login sonrası tenant seçimi, tenant'a özel JWT döner
- POST /otp/send       → Mülk sahibi için telefon numarasına OTP gönder (şimdilik OTP'yi loglara yaz, SMS entegrasyonu yok)
- POST /otp/verify     → OTP doğrula, PhonePropertyMap'e bakarak erişilebilir mülk listesini claim'e ekle ve JWT döner

### 2. TenantsController  /api/tenants
- GET    /             → Tüm tenantları listele (sadece SuperAdmin)
- POST   /             → Yeni tenant oluştur
- PUT    /{id}         → Tenant güncelle
- GET    /{id}/admins  → Tenant'a atanmış adminleri listele
- POST   /{id}/admins  → Tenant'a admin ata

### 3. PropertiesController  /api/properties
- GET    /             → Mülk listesi (TenantId otomatik filtre)
- GET    /{id}         → Mülk detayı + aktif sakin bilgisi
- POST   /             → Yeni mülk tanımla
- PUT    /{id}         → Mülk güncelle
- DELETE /{id}         → Soft delete (IsActive = false)
- POST   /{id}/assign-resident  → Mülke yeni sakin ata (eski IsActive=false, PhonePropertyMap güncelle)

### 4. ResidentsController  /api/residents
- GET    /             → Sakin listesi (filtre: propertyId, residentType, isActive)
- GET    /{id}         → Sakin detayı
- POST   /             → Yeni sakin ekle
- PUT    /{id}         → Sakin bilgisi güncelle
- DELETE /{id}         → Soft delete

### 5. DuesController  /api/dues
- GET    /definitions         → Aidat tanımları listesi
- POST   /definitions         → Yeni aidat tanımı oluştur
- PUT    /definitions/{id}    → Aidat tanımı güncelle
- POST   /charge/bulk         → Seçili ay için tüm aktif mülklere toplu borçlandırma
- POST   /charge/single       → Tekil mülke manuel borç ekle

### 6. ChargesController  /api/charges
- GET    /             → Borç listesi (filtre: propertyId, status, year, month)
- GET    /{id}         → Borç detayı
- GET    /summary      → Toplam borç, tahsilat, gecikmiş özet (dashboard için)

### 7. PaymentsController  /api/payments
- GET    /             → Tahsilat listesi (filtre: propertyId, dateFrom, dateTo)
- GET    /{id}         → Tahsilat detayı
- POST   /             → Tahsilat kaydet (ChargeId, PaidAmount, PaymentMethod)
                         Partial payment desteği: Charges.PaidAmount güncellenir,
                         PaidAmount == Amount ise Status = 'Paid', değilse 'Partial'
- DELETE /{id}         → İptal (soft — ayrı bir Cancelled status veya silme)

### 8. NotificationsController  /api/notifications
- GET    /             → Bildirim geçmişi
- POST   /             → Bildirim gönder (TargetType: All | Block | Property | Resident)
                         FCM gönderimi şimdilik loglara yaz, servis interface hazır olsun

### 9. ReportsController  /api/reports
- GET    /monthly-collection    → Aylık tahsilat özeti (year, month parametresi)
- GET    /debt-aging            → Borç yaşlandırma raporu (0-30, 31-60, 61-90, 90+ gün)
- GET    /property-summary      → Daire bazlı borç/tahsilat özeti
- GET    /overdue               → Vadesi geçmiş borçlar listesi

### 10. PhoneMapController  /api/phonemap
- GET    /my-properties  → JWT'deki telefon numarasına göre erişilebilir mülk listesi
                           (mülk sahibi uygulaması için, OTP JWT gerektirir)

---

## BACKGROUND JOB

BackgroundJobs/MonthlyChargeJob.cs
- IHostedService implement et
- Her ayın 1'inde 08:00'de çalışacak şekilde zamanlayıcı kur
- DuesDefinitions tablosundaki aktif Monthly tanımları için
  tüm aktif Properties'e Charges kaydı oluştur
- Duplicate önlemi: UQ_Charge_Period constraint'e takılırsa o kaydı atla, devam et
- Her çalışmada log yaz (Microsoft.Extensions.Logging)

---

## GENEL KURALLAR

- Response için ApiResponse<T> generic wrapper kullan: { success, data, message, errors }
- Validation için FluentValidation kullan
- Hata yönetimi için global exception middleware yaz
- appsettings.json'a JWT, ConnectionString ve Logging ayarlarını ekle
- Program.cs'de tüm servisleri kaydet, Swagger ekle (JWT Bearer destekli)
- Swagger'da her endpoint'in hangi role'e açık olduğunu summary ile belirt
- Controller'larda business logic olmasın, sadece servis çağrısı
- Repository'ler sadece Dapper ile veri erişimi yapsın, business logic servis katmanında olsun
- Tüm Dapper sorgularında parametreli sorgu kullan, string interpolation kullanma
- Tüm entity sınıfları Models/Entities/ altında, DTO'lar Models/DTOs/ altında olsun

---

Önce klasör yapısını ve SQL script'ini oluştur, 
sonra Infrastructure katmanını, 
sonra Repositories, 
sonra Services, 
sonra Controllers sırasıyla ilerle.
Her adımda ne yaptığını kısaca belirt.