## ChatSupport – Canlı Destek Sistemi

Bu proje, web sitelerine entegre edilebilen gerçek zamanlı canlı destek servisidir. SignalR ile anlık mesajlaşma, REST API ile yönetim ve MySQL ile kalıcı depolama sağlar.

## Özellikler

- SignalR ile gerçek zamanlı mesajlaşma
- REST API (oturumlar, mesajlar, istatistik)
- Otomatik yeniden bağlanma (client)
- MySQL 8+ veritabanı
- Basit yönetim paneli ve kullanıcı widget’ı

## Gereksinimler

- .NET SDK 9.0
- MySQL Server 8.0+

## Kurulum (Yerel)

1) Veritabanı:

```sql
CREATE DATABASE IF NOT EXISTS ChatDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER IF NOT EXISTS 'chatuser'@'localhost' IDENTIFIED BY '12345';
GRANT ALL PRIVILEGES ON ChatDb.* TO 'chatuser'@'localhost';
FLUSH PRIVILEGES;
```

2) Bağlantı:

`appsettings.json` içindeki connection string’i kontrol edin:

```json
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=127.0.0.1;Port=3306;Database=ChatDb;Uid=chatuser;Pwd=12345;"
  }
}
```

3) Çalıştırma:

```bash
dotnet restore
dotnet run
```

Varsayılan adresler (launchSettings’e göre):

- Admin Giriş: http://localhost:5122/admin_login.html
- Admin Panel: http://localhost:5122/index_admin.html
- Kullanıcı Chat: http://localhost:5122/index_user_popup.html
- Swagger: http://localhost:5122/swagger

Admin demo bilgisi (yalnızca geliştirme için): `admin` / `admin123`

## Entegrasyon (Widget)

HTML dosyanızdaki `</body>` öncesine ekleyin:

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.5/signalr.min.js"></script>
<script src="http://YOUR_API_URL/chatsupport-widget.js" data-api-url="http://YOUR_API_URL" data-position="bottom-right" data-color="#ffc107"></script>
```

## API Özet

- GET `/api/chat/sessions`
- GET `/api/chat/sessions/open`
- GET `/api/chat/sessions/claimed`
- GET `/api/chat/{chatId}`
- GET `/api/chat/{chatId}/messages`
- DELETE `/api/chat/{chatId}`
- GET `/api/chat/stats`

## Production

### Windows Server (IIS)

1. **Projeyi Publish Edin:**
```bash
dotnet publish -c Release -o ./publish
```

2. **IIS Kurulumu:**
   - IIS'te yeni bir site oluşturun
   - Application Pool'u ".NET CLR Version" olarak "No Managed Code" seçin
   - Physical path'i publish klasörüne ayarlayın
   - `.NET Core Hosting Bundle` yükleyin

3. **Connection String Güncelleme:**
   - `appsettings.json` içindeki connection string'i production MySQL bilgileriyle güncelleyin

### Linux (Ubuntu + Nginx)

1. **Projeyi Publish Edin:**
```bash
dotnet publish -c Release -o ./publish
```

2. **Dosyaları Sunucuya Aktarın:**
```bash
scp -r ./publish user@server:/var/www/chatsupport
```

3. **Systemd Service Oluşturun:**
```bash
sudo nano /etc/systemd/system/chatsupport.service
```

```ini
[Unit]
Description=ChatSupport API

[Service]
WorkingDirectory=/var/www/chatsupport
ExecStart=/usr/bin/dotnet /var/www/chatsupport/ChatSupport.dll
Restart=always
RestartSec=10
SyslogIdentifier=chatsupport
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

4. **Service'i Başlatın:**
```bash
sudo systemctl enable chatsupport
sudo systemctl start chatsupport
```

5. **Nginx Yapılandırması:**
```nginx
server {
    listen 80;
    server_name yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Docker

1. **Dockerfile Oluşturun:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ChatSupport.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatSupport.dll"]
```

2. **Docker Compose (docker-compose.yml):**
```yaml
version: '3.8'
services:
  mysql:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword
      MYSQL_DATABASE: ChatDb
      MYSQL_USER: chatuser
      MYSQL_PASSWORD: 12345
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

  chatsupport:
    build: .
    ports:
      - "5000:5000"
    depends_on:
      - mysql
    environment:
      ConnectionStrings__MySqlConnection: "Server=mysql;Port=3306;Database=ChatDb;Uid=chatuser;Pwd=12345;"

volumes:
  mysql_data:
```

3. **Çalıştırın:**
```bash
docker-compose up -d
```

## Güvenlik

### Production için MUTLAKA yapılması gerekenler:

1. **Admin Şifresini Değiştirin:**
   - `admin_login.html` içinde sabit kodlanmış şifre yerine backend authentication kullanın

2. **HTTPS Kullanın:**
   - Let's Encrypt ile ücretsiz SSL sertifikası alın
   - `appsettings.json` içinde HTTPS redirect ekleyin

3. **CORS Politikasını Güncelleyin:**
```csharp
// Program.cs içinde
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

4. **Database Connection String'i Şifreleyin:**
   - Azure Key Vault, AWS Secrets Manager veya environment variables kullanın

5. **Rate Limiting Ekleyin:**
```bash
dotnet add package AspNetCoreRateLimit
```

6. **Logging ve Monitoring:**
   - Serilog, Application Insights veya ELK Stack kullanın

## Test

### Manuel Test

1. **Admin Panel Testi:**
   - http://localhost:5000/admin_login.html adresine gidin
   - Admin bilgileriyle giriş yapın
   - Admin panel açıldığını doğrulayın

2. **Kullanıcı Chat Testi:**
   - http://localhost:5000/index_user_popup.html adresine gidin
   - Adınızı girin ve sohbet başlatın
   - Admin panelde yeni sohbetin göründüğünü kontrol edin
   - Admin panelden sohbeti "Claim" edin
   - Mesaj alışverişi yapın
   - Sohbeti sonlandırın

3. **Widget Testi:**
   - Test HTML sayfası oluşturun
   - Widget script'ini ekleyin
   - Widget'ın düzgün yüklendiğini kontrol edin

### API Test (Swagger ile)

1. http://localhost:5000/swagger adresine gidin
2. `/api/chat/stats` endpoint'ini test edin
3. `/api/chat/sessions/open` endpoint'ini test edin

## Performans

1. **SignalR Ölçeklendirme (Azure SignalR Service):**
```bash
dotnet add package Microsoft.Azure.SignalR
```

2. **Redis Cache:**
```bash
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

3. **Database Indexleme:**
```sql
CREATE INDEX idx_chatsessions_claimed ON ChatSessions(ClaimedBySupport);
CREATE INDEX idx_chatmessages_chatid ON ChatMessages(ChatId);
CREATE INDEX idx_chatmessages_timestamp ON ChatMessages(Timestamp);
```

## Sorun Giderme

### MySQL Bağlantı Hatası
```
Error: Unable to connect to MySQL server
```
**Çözüm:**
- MySQL servisinin çalıştığını kontrol edin: `sudo systemctl status mysql`
- Connection string'in doğru olduğunu kontrol edin
- Firewall ayarlarını kontrol edin

### SignalR Bağlantı Hatası
```
Error: Failed to start the connection
```
**Çözüm:**
- CORS ayarlarını kontrol edin
- API URL'in doğru olduğunu kontrol edin
- Browser console'da hata loglarını inceleyin

### Migration Hatası
```
Error: Unable to create migrations
```
**Çözüm:**
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Lisans ve Katkı

- Lisans: MIT (isteğe bağlı olarak güncelleyebilirsiniz)
- Katkı: Pull request ve issue açabilirsiniz

