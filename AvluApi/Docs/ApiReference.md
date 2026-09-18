# AvluApi - API Referans Dokumani

Base URL: `https://<sunucu>/api`

Tum basarili yanitlar `ApiResponse<T>` formatindadir:

```json
{
  "success": true,
  "data": {},
  "message": "...",
  "errors": null
}
```

Hata yanitlari:

```json
{
  "success": false,
  "data": null,
  "message": "...",
  "errors": ["..."]
}
```

## Token Tipleri

| Tip | Nereden Alinir | Claims | Not |
|---|---|---|---|
| Pre-Auth Token | `POST /api/auth/login` | `adminId`, `username`, `tenant` | Tenant secimi oncesi |
| Tenant Token | `POST /api/auth/tenant-select` | `adminId`, `tenantId`, `role` | Admin/yonetim islemleri |
| Resident Token | `POST /api/auth/otp/verify` | `phoneNumber`, `tenantId`, `propertyId` | Mulk sahibi islemleri |

## Icindekiler

1. Auth (`/api/auth`) — register, check-code, login, tenant-select, otp/send, otp/verify
2. Tenants (`/api/tenants`)
3. Properties (`/api/properties`)
4. Residents (`/api/residents`)
5. Dues (`/api/dues`)
6. Charges (`/api/charges`)
7. Payments (`/api/payments`)
8. Notifications (`/api/notifications`)
9. Reports (`/api/reports`)
10. PhoneMap (`/api/phonemap`)

---

## Auth

### POST /api/auth/register
Aciklama: Yeni bir site ve yonetici hesabi olusturur. Kayit sonrasi /api/auth/login ile giris yapilir.
Yetki: Public

Ornek istek:
```json
{
  "tenantName": "Kadikoy Gunes Sitesi",
  "tenantCode": "kadikoy-gunes",
  "address": "Kadikoy, Istanbul",
  "username": "ahmet.yilmaz",
  "password": "sifre123",
  "email": "ahmet@example.com"
}
```

Alanlar:
| Alan | Zorunlu | Kural |
|---|---|---|
| tenantName | Evet | max 200 karakter |
| tenantCode | Evet | max 50 karakter, benzersiz olmali |
| address | Hayir | max 500 karakter |
| username | Evet | max 100 karakter, benzersiz olmali |
| password | Evet | min 6, max 100 karakter |
| email | Hayir | gecerli e-posta formati, max 200 karakter |

Ornek yanit (basarili):
```json
{ "success": true, "data": "Kayit basarili. Giris yapabilirsiniz.", "message": null, "errors": null }
```

Olasi hatalar:
- `400` — `"Bu site kodu zaten kullaniliyor."` (tenantCode cakismasi)
- `400` — `"Bu kullanici adi zaten alinmis."` (username cakismasi)
- `400` — Model dogrulama hatasi (zorunlu alan bos, sifre cok kisa vb.)

---

### GET /api/auth/check-code/{code}
Aciklama: Verilen site kodunun kullanilabilir olup olmadigini kontrol eder. MAUI kayit ekraninda anlık dogrulama icin kullanilir.
Yetki: Public

Ornek istek:
```bash
GET /api/auth/check-code/kadikoy-gunes
```

Ornek yanit (musait):
```json
{ "success": true, "data": true, "message": null, "errors": null }
```

Ornek yanit (kullanımda):
```json
{ "success": true, "data": false, "message": null, "errors": null }
```

Not: `data: true` = kullanilabilir, `data: false` = zaten alinmis.

---

### POST /api/auth/login
Aciklama: Admin girisi yapar, Pre-Auth token dondurur.
Yetki: Public

Ornek istek:
```json
{ "username": "admin", "password": "123456" }
```

Ornek yanit:
```json
{ "success": true, "data": "<jwt>", "message": "Giris basarili.", "errors": null }
```

### POST /api/auth/tenant-select
Aciklama: Pre-Auth token ile aktif tenant secer, Tenant token dondurur.
Yetki: Bearer (Pre-Auth)

Ornek istek:
```json
{ "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }
```

Ornek yanit:
```json
{ "success": true, "data": "<tenant-jwt>", "message": "Tenant secildi.", "errors": null }
```

### POST /api/auth/otp/send
Aciklama: Tenant token ile telefona OTP gonderme islemini baslatir.
Yetki: Bearer (Tenant)

Ornek istek:
```json
{ "phoneNumber": "+905551112233" }
```

Ornek yanit:
```json
{ "success": true, "data": "OTP gonderildi.", "message": "OTP loglanmistir.", "errors": null }
```

### POST /api/auth/otp/verify
Aciklama: OTP dogrular, Resident token dondurur.
Yetki: Public

Ornek istek:
```json
{
  "phoneNumber": "+905551112233",
  "otp": "123456",
  "tenantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

Ornek yanit:
```json
{ "success": true, "data": "<resident-jwt>", "message": "OTP dogrulandi.", "errors": null }
```

---

## Tenants

### GET /api/tenants
Aciklama: Tum tenantlari listeler.
Yetki: Bearer

Ornek istek:
```bash
curl -H "Authorization: Bearer <token>" https://<sunucu>/api/tenants
```

Ornek yanit:
```json
{ "success": true, "data": [{ "tenantId": "...", "tenantName": "Gul Apartmani" }], "message": null, "errors": null }
```

### POST /api/tenants
Aciklama: Yeni tenant olusturur.
Yetki: Bearer

Ornek istek:
```json
{ "tenantName": "Lale Sitesi", "tenantCode": "LALE-01", "address": "Ankara" }
```

Ornek yanit:
```json
{ "success": true, "data": { "tenantId": "...", "tenantName": "Lale Sitesi" }, "message": "Tenant olusturuldu.", "errors": null }
```

### PUT /api/tenants/{id:guid}
Aciklama: Tenant bilgilerini gunceller.
Yetki: Bearer

Ornek istek:
```json
{ "tenantName": "Lale Sitesi 2", "address": "Istanbul", "isActive": true }
```

Ornek yanit:
```json
{ "success": true, "data": { "tenantId": "...", "tenantName": "Lale Sitesi 2" }, "message": "Tenant guncellendi.", "errors": null }
```

### GET /api/tenants/{id:guid}/admins
Aciklama: Tenanta atanmis adminleri listeler.
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": [{ "adminId": 1, "username": "yonetici", "role": "Manager" }], "message": null, "errors": null }
```

### POST /api/tenants/{id:guid}/admins
Aciklama: Tenanta admin atar.
Yetki: Bearer

Ornek istek:
```json
{ "adminId": 2, "role": "Manager" }
```

Ornek yanit:
```json
{ "success": true, "data": "Admin atandi.", "message": null, "errors": null }
```

---

## Properties

### GET /api/properties
Aciklama: Aktif tenantin mulklerini listeler.
Yetki: Bearer
Tenant Scope: Evet

Ornek yanit:
```json
{ "success": true, "data": [{ "propertyId": 1, "blockName": "A", "doorNumber": "5" }], "message": null, "errors": null }
```

### GET /api/properties/{id:long}
Aciklama: Tek mulk detayi getirir.
Yetki: Bearer
Tenant Scope: Evet

Ornek yanit:
```json
{ "success": true, "data": { "propertyId": 1, "doorNumber": "5", "activeResident": null }, "message": null, "errors": null }
```

### POST /api/properties
Aciklama: Yeni mulk olusturur.
Yetki: Bearer
Tenant Scope: Evet

Ornek istek:
```json
{ "blockName": "B", "doorNumber": "12", "floor": 3, "propertyType": "Apartment" }
```

Ornek yanit:
```json
{ "success": true, "data": { "propertyId": 7, "doorNumber": "12" }, "message": "Mulk olusturuldu.", "errors": null }
```

### PUT /api/properties/{id:long}
Aciklama: Mulk gunceller.
Yetki: Bearer
Tenant Scope: Evet

Ornek istek:
```json
{ "blockName": "B", "doorNumber": "12", "floor": 4, "propertyType": "Commercial" }
```

Ornek yanit:
```json
{ "success": true, "data": { "propertyId": 7, "propertyType": "Commercial" }, "message": "Mulk guncellendi.", "errors": null }
```

### DELETE /api/properties/{id:long}
Aciklama: Mulku soft-delete yapar.
Yetki: Bearer
Tenant Scope: Evet

Ornek yanit:
```json
{ "success": true, "data": "Mulk silindi.", "message": null, "errors": null }
```

### POST /api/properties/{id:long}/assign-resident
Aciklama: Mulke sakin atar.
Yetki: Bearer
Tenant Scope: Evet

Ornek istek:
```json
{
  "fullName": "Fatma Kaya",
  "phoneNumber": "+905551234567",
  "residentType": "Tenant",
  "moveInDate": "2026-01-10"
}
```

Ornek yanit:
```json
{ "success": true, "data": { "residentId": 15, "propertyId": 7, "fullName": "Fatma Kaya" }, "message": "Sakin atandi.", "errors": null }
```

---

## Residents

### GET /api/residents
Aciklama: Sakinleri listeler.
Yetki: Bearer
Tenant Scope: Evet
Query: `propertyId?`, `residentType?`, `isActive?`

Ornek istek:
```bash
curl -H "Authorization: Bearer <token>" "https://<sunucu>/api/residents?propertyId=7&isActive=true"
```

Ornek yanit:
```json
{ "success": true, "data": [{ "residentId": 15, "fullName": "Fatma Kaya", "residentType": "Tenant" }], "message": null, "errors": null }
```

### GET /api/residents/{id:long}
Aciklama: Tek sakin detayi.
Yetki: Bearer
Tenant Scope: Evet

Ornek yanit:
```json
{ "success": true, "data": { "residentId": 15, "propertyId": 7, "fullName": "Fatma Kaya" }, "message": null, "errors": null }
```

### POST /api/residents
Aciklama: Yeni sakin kaydi olusturur.
Yetki: Bearer
Tenant Scope: Evet

Ornek istek:
```json
{
  "propertyId": 7,
  "fullName": "Mehmet Demir",
  "phoneNumber": "+905559999999",
  "residentType": "Owner",
  "moveInDate": "2025-11-01"
}
```

Ornek yanit:
```json
{ "success": true, "data": { "residentId": 16, "propertyId": 7 }, "message": "Sakin eklendi.", "errors": null }
```

### PUT /api/residents/{id:long}
Aciklama: Sakin kaydini gunceller.
Yetki: Bearer
Tenant Scope: Evet

Ornek istek:
```json
{
  "fullName": "Mehmet Demir",
  "phoneNumber": "+905559999999",
  "residentType": "Owner",
  "moveInDate": "2025-11-01",
  "moveOutDate": null
}
```

Ornek yanit:
```json
{ "success": true, "data": { "residentId": 16, "fullName": "Mehmet Demir" }, "message": "Sakin guncellendi.", "errors": null }
```

### DELETE /api/residents/{id:long}
Aciklama: Sakini soft-delete yapar.
Yetki: Bearer
Tenant Scope: Evet

Ornek yanit:
```json
{ "success": true, "data": "Sakin silindi.", "message": null, "errors": null }
```

---

## Dues

### GET /api/dues/definitions
Aciklama: Aidat tanimlarini listeler.
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": [{ "duesDefId": 1, "name": "Aylik Aidat", "amount": 500.0 }], "message": null, "errors": null }
```

### POST /api/dues/definitions
Aciklama: Aidat tanimi olusturur.
Yetki: Bearer

Ornek istek:
```json
{ "name": "Aylik Aidat", "amount": 500.0, "dueDay": 5, "periodType": "Monthly" }
```

Ornek yanit:
```json
{ "success": true, "data": { "duesDefId": 2, "name": "Aylik Aidat" }, "message": "Aidat tanimi olusturuldu.", "errors": null }
```

### PUT /api/dues/definitions/{id:long}
Aciklama: Aidat tanimini gunceller.
Yetki: Bearer

Ornek istek:
```json
{ "name": "Aylik Aidat", "amount": 550.0, "dueDay": 7, "isActive": true }
```

Ornek yanit:
```json
{ "success": true, "data": { "duesDefId": 2, "amount": 550.0 }, "message": "Aidat tanimi guncellendi.", "errors": null }
```

### POST /api/dues/charge/bulk
Aciklama: Toplu borclandirma yapar.
Yetki: Bearer

Ornek istek:
```json
{ "year": 2026, "month": 4, "duesDefId": 2 }
```

Ornek yanit:
```json
{ "success": true, "data": "Toplu borclandirma tamamlandi.", "message": null, "errors": null }
```

### POST /api/dues/charge/single
Aciklama: Tekil borc olusturur.
Yetki: Bearer

Ornek istek:
```json
{
  "propertyId": 7,
  "duesDefId": 2,
  "description": "Ek gider",
  "amount": 300.0,
  "dueDate": "2026-04-15",
  "periodYear": 2026,
  "periodMonth": 4
}
```

Ornek yanit:
```json
{ "success": true, "data": { "chargeId": 120, "propertyId": 7, "amount": 300.0 }, "message": "Borc kaydedildi.", "errors": null }
```

---

## Charges

### GET /api/charges
Aciklama: Borc listesi.
Yetki: Bearer
Query: `propertyId?`, `status?`, `year?`, `month?`

Ornek istek:
```bash
curl -H "Authorization: Bearer <token>" "https://<sunucu>/api/charges?propertyId=7&status=Pending&year=2026&month=4"
```

Ornek yanit:
```json
{ "success": true, "data": [{ "chargeId": 120, "propertyId": 7, "amount": 300.0, "status": "Pending" }], "message": null, "errors": null }
```

### GET /api/charges/{id:long}
Aciklama: Tek borc detayi.
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": { "chargeId": 120, "propertyId": 7, "amount": 300.0 }, "message": null, "errors": null }
```

### GET /api/charges/summary
Aciklama: Dashboard ozetini dondurur.
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": { "totalDebt": 120000.0, "collected": 85000.0, "overdue": 15000.0 }, "message": null, "errors": null }
```

---

## Payments

### GET /api/payments
Aciklama: Tahsilat listesini getirir.
Yetki: Bearer
Query: `propertyId?`, `dateFrom?`, `dateTo?`

Ornek istek:
```bash
curl -H "Authorization: Bearer <token>" "https://<sunucu>/api/payments?propertyId=7&dateFrom=2026-04-01&dateTo=2026-04-30"
```

Ornek yanit:
```json
{ "success": true, "data": [{ "paymentId": 90, "chargeId": 120, "paidAmount": 300.0 }], "message": null, "errors": null }
```

### GET /api/payments/{id:long}
Aciklama: Tek tahsilat detayi.
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": { "paymentId": 90, "chargeId": 120, "paidAmount": 300.0, "paymentMethod": "Cash" }, "message": null, "errors": null }
```

### POST /api/payments
Aciklama: Tahsilat kaydi olusturur (kismi odeme destekli).
Yetki: Bearer

Ornek istek:
```json
{
  "chargeId": 120,
  "paidAmount": 150.0,
  "paymentMethod": "BankTransfer",
  "receiptNumber": "RCPT-2026-0045",
  "notes": "Ilk taksit"
}
```

Ornek yanit:
```json
{ "success": true, "data": { "paymentId": 91, "chargeId": 120, "paidAmount": 150.0 }, "message": "Tahsilat kaydedildi.", "errors": null }
```

### DELETE /api/payments/{id:long}
Aciklama: Tahsilat kaydini iptal eder.
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": "Tahsilat iptal edildi.", "message": null, "errors": null }
```

---

## Notifications

### GET /api/notifications
Aciklama: Bildirim gecmisini listeler.
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": [{ "notifId": 10, "title": "Su Kesintisi", "targetType": "All" }], "message": null, "errors": null }
```

### POST /api/notifications
Aciklama: Yeni bildirim gonderir.
Yetki: Bearer

Ornek istek:
```json
{ "title": "Duyuru", "body": "Asansor bakimi yarin", "targetType": "Block", "targetId": "A" }
```

Ornek yanit:
```json
{ "success": true, "data": { "notifId": 11, "title": "Duyuru" }, "message": "Bildirim gonderildi.", "errors": null }
```

---

## Reports

### GET /api/reports/monthly-collection
Aciklama: Aylik tahsilat raporu.
Yetki: Bearer
Query: `year` (zorunlu), `month` (zorunlu)

Ornek istek:
```bash
curl -H "Authorization: Bearer <token>" "https://<sunucu>/api/reports/monthly-collection?year=2026&month=4"
```

Ornek yanit:
```json
{ "success": true, "data": { "year": 2026, "month": 4, "totalDebt": 50000.0, "collected": 42000.0 }, "message": null, "errors": null }
```

### GET /api/reports/debt-aging
Aciklama: Borc yaslandirma raporu (0-30,31-60,61-90,90+).
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": { "bucket0_30": 12000.0, "bucket31_60": 8000.0, "bucket61_90": 4000.0, "bucket90Plus": 2000.0 }, "message": null, "errors": null }
```

### GET /api/reports/property-summary
Aciklama: Daire bazli borc/tahsilat ozeti.
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": [{ "propertyId": 7, "totalDebt": 5000.0, "collected": 4500.0 }], "message": null, "errors": null }
```

### GET /api/reports/overdue
Aciklama: Vadesi gecmis borclari listeler.
Yetki: Bearer

Ornek yanit:
```json
{ "success": true, "data": [{ "chargeId": 80, "propertyId": 4, "remainingAmount": 600.0, "daysOverdue": 45 }], "message": null, "errors": null }
```

---

## PhoneMap

### GET /api/phonemap/my-properties
Aciklama: Resident token icindeki `phoneNumber` ile iliskili mulkleri listeler.
Yetki: Bearer (Resident Token)
Tenant Scope: Evet

Ornek istek:
```bash
curl -H "Authorization: Bearer <resident-token>" https://<sunucu>/api/phonemap/my-properties
```

Ornek yanit:
```json
{ "success": true, "data": [{ "propertyId": 7, "blockName": "B", "doorNumber": "12" }], "message": null, "errors": null }
```

---

## Hata Kodlari

| Kod | Anlam |
|---|---|
| 200 | Basarili islem |
| 201 | Kayit olusturuldu |
| 400 | Gecersiz istek / dogrulama hatasi |
| 401 | Yetkisiz |
| 403 | Erisim engeli (claim/token tipi uyumsuz) |
| 404 | Kayit bulunamadi |
| 500 | Sunucu hatasi |

Notlar:
- `TenantMiddleware` nedeniyle `tenantId` claim'i olmayan tokenlarla bircok endpoint `403` donebilir.
- `ExceptionMiddleware` belirli exception tiplerini 400/401/404 olarak mapler.
