-- ChatSupport Database Setup Script
-- Bu script'i MySQL'de çalıştırarak veritabanını hazırlayın

-- Veritabanı oluştur
CREATE DATABASE IF NOT EXISTS ChatDb 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

-- Kullanıcı oluştur (eğer yoksa)
CREATE USER IF NOT EXISTS 'chatuser'@'localhost' IDENTIFIED BY '12345';

-- Yetkileri ver
GRANT ALL PRIVILEGES ON ChatDb.* TO 'chatuser'@'localhost';

-- Değişiklikleri uygula
FLUSH PRIVILEGES;

-- Veritabanını seç
USE ChatDb;

-- Başarı mesajı
SELECT 'Database setup completed successfully!' AS Status;

-- Kullanıcıyı göster
SELECT User, Host FROM mysql.user WHERE User = 'chatuser';

-- NOTLAR:
-- 1. Production ortamında güçlü bir şifre kullanın!
-- 2. Migrations otomatik olarak çalışacaktır (Program.cs içinde)
-- 3. Manuel migration için: dotnet ef database update

