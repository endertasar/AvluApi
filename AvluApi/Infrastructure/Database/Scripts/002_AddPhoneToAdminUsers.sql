-- 002_AddPhoneToAdminUsers.sql
-- AdminUsers tablosuna PhoneNumber kolonu ekler.
-- Mevcut kayıtlar için NULL kabul edilir; yeni kayıtlar uygulama katmanında zorunlu tutulur.

ALTER TABLE AdminUsers
    ADD PhoneNumber NVARCHAR(20) NULL;
