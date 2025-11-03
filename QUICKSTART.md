# Hızlı Başlangıç

Bu dosya, geliştirme ortamında ChatSupport'u hızlıca ayağa kaldırmak için kısa adımları içerir.

## 1) MySQL

```bash
mysql -u root -p
```

```sql
CREATE DATABASE ChatDb;
CREATE USER 'chatuser'@'localhost' IDENTIFIED BY '12345';
GRANT ALL PRIVILEGES ON ChatDb.* TO 'chatuser'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

## 2) Projeyi Çalıştırın

```bash
cd ChatSupport
dotnet run
```

Varsayılan adresler (launchSettings.json'a göre):

- Admin Giriş: http://localhost:5122/admin_login.html
- Admin Panel: http://localhost:5122/index_admin.html
- Kullanıcı Chat: http://localhost:5122/index_user_popup.html
- Swagger: http://localhost:5122/swagger

## 3) Test

1. **Admin Paneline Giriş:**
   - Tarayıcıda aç: http://localhost:5000/admin_login.html
   - Kullanıcı: `admin`, Şifre: `admin123`

2. **Kullanıcı Olarak Test:**
   - Yeni sekmede aç: http://localhost:5000/index_user_popup.html
   - Adınızı girin ve mesaj gönderin

3. **Admin Panelde İşlem:**
   - Admin panelde yeni sohbeti görün
   - "Claim" butonuna tıklayın
   - Kullanıcıyla konuşun

## Entegrasyon

HTML dosyanızın `</body>` etiketinden önce:

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js"></script>
<script 
    src="http://localhost:5000/chatsupport-widget.js" 
    data-api-url="http://localhost:5000"
    data-position="bottom-right"
    data-color="#ffc107">
</script>
```

## SSS

**S: MySQL yüklü değil, nasıl kurarım?**

Windows:
```
https://dev.mysql.com/downloads/installer/
```

Mac:
```bash
brew install mysql
brew services start mysql
```

Ubuntu/Linux:
```bash
sudo apt update
sudo apt install mysql-server
sudo systemctl start mysql
```

**S: Port 5000 kullanımda, nasıl değiştiririm?**

`Properties/launchSettings.json` dosyasında:
```json
"applicationUrl": "http://localhost:YOUR_PORT"
```

**S: Production'a nasıl deploy ederim?**

Detaylar için `README.md` içindeki "Production" bölümüne bakın.

## Yardım mı Lazım?

- 📖 Detaylı döküman: `README.md`
- 🐛 Sorun mu var?: `README.md` içindeki "Sorun Giderme" bölümü
- 💬 API Dökümantasyonu: http://localhost:5000/swagger

