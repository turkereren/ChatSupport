# 🚀 Production Deployment Rehberi

Bu dosya, ChatSupport uygulamasını production ortamına deploy etmek için detaylı adımları içerir.

## 📋 Ön Hazırlık

### 1. Gereksinimler Kontrolü

- [ ] .NET 9.0 Runtime yüklü
- [ ] MySQL 8.0+ kurulu ve çalışır durumda
- [ ] SSL sertifikası hazır (Let's Encrypt önerilir)
- [ ] Domain adı yapılandırılmış
- [ ] Firewall portları açık (80, 443, 3306)

### 2. Güvenlik Kontrol Listesi

- [ ] Admin şifresi değiştirildi
- [ ] Database şifresi güçlü bir şifre ile değiştirildi
- [ ] CORS policy'si güncellendi (sadece kendi domain'iniz)
- [ ] HTTPS yapılandırıldı
- [ ] Connection string environment variable'a taşındı
- [ ] appsettings.Production.json hassas bilgiler olmadan

## 🔧 Deployment Adımları

### A. Windows Server + IIS

#### 1. IIS Hazırlığı

```powershell
# IIS ve gerekli özellikleri yükle
Install-WindowsFeature -name Web-Server -IncludeManagementTools
Install-WindowsFeature -name Web-WebSockets

# ASP.NET Core Hosting Bundle yükle
# https://dotnet.microsoft.com/download/dotnet/9.0
# - hosting bundle dosyasını indirin ve çalıştırın
```

#### 2. Projeyi Publish Etme

```powershell
# Proje klasöründe
dotnet publish -c Release -o C:\inetpub\wwwroot\chatsupport

# appsettings.Production.json'ı kopyala
Copy-Item appsettings.Production.json.example C:\inetpub\wwwroot\chatsupport\appsettings.Production.json
```

#### 3. appsettings.Production.json Güncelleme

```json
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3306;Database=ChatDb;Uid=chatuser;Pwd=GÜÇLÜ_ŞİFRE_BURAYA;"
  }
}
```

#### 4. IIS Site Oluşturma

1. IIS Manager'ı aç
2. Sites → Add Website
   - Site name: ChatSupport
   - Physical path: `C:\inetpub\wwwroot\chatsupport`
   - Binding: http / Port 80 / yourdomain.com
3. Application Pool ayarları:
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated
   - Identity: ApplicationPoolIdentity

#### 5. SSL Sertifikası

```powershell
# Let's Encrypt kullanarak (win-acme)
# https://www.win-acme.com/ indir

.\wacs.exe --target iis --siteid 1
```

#### 6. Web.config Oluştur (Otomatik oluşturulur ama kontrol edin)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <handlers>
      <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
    </handlers>
    <aspNetCore processPath="dotnet" 
                arguments=".\ChatSupport.dll" 
                stdoutLogEnabled="true" 
                stdoutLogFile=".\logs\stdout" 
                hostingModel="inprocess">
      <environmentVariables>
        <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
      </environmentVariables>
    </aspNetCore>
  </system.webServer>
</configuration>
```

---

### B. Linux Server (Ubuntu 22.04 + Nginx)

#### 1. Sunucu Hazırlığı

```bash
# Sistemi güncelle
sudo apt update && sudo apt upgrade -y

# .NET Runtime yükle
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt update
sudo apt install -y aspnetcore-runtime-9.0

# Nginx yükle
sudo apt install -y nginx

# MySQL yükle
sudo apt install -y mysql-server
```

#### 2. MySQL Yapılandırma

```bash
# MySQL güvenlik yapılandırması
sudo mysql_secure_installation

# MySQL'e bağlan
sudo mysql

# Veritabanı oluştur
CREATE DATABASE ChatDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'chatuser'@'localhost' IDENTIFIED BY 'GÜÇLÜ_ŞİFRE';
GRANT ALL PRIVILEGES ON ChatDb.* TO 'chatuser'@'localhost';
FLUSH PRIVILEGES;
EXIT;
```

#### 3. Projeyi Publish ve Transfer

**Local makinede:**
```bash
dotnet publish -c Release -o ./publish

# Dosyaları sunucuya gönder
scp -r ./publish user@your-server:/tmp/chatsupport
```

**Sunucuda:**
```bash
# Uygulama klasörü oluştur
sudo mkdir -p /var/www/chatsupport
sudo mv /tmp/chatsupport/* /var/www/chatsupport/

# Yetkileri ayarla
sudo chown -R www-data:www-data /var/www/chatsupport
sudo chmod -R 755 /var/www/chatsupport
```

#### 4. appsettings.Production.json Oluştur

```bash
sudo nano /var/www/chatsupport/appsettings.Production.json
```

```json
{
  "ConnectionStrings": {
    "MySqlConnection": "Server=localhost;Port=3306;Database=ChatDb;Uid=chatuser;Pwd=GÜÇLÜ_ŞİFRE;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

#### 5. Systemd Service Oluştur

```bash
sudo nano /etc/systemd/system/chatsupport.service
```

```ini
[Unit]
Description=ChatSupport Live Support API
After=network.target mysql.service

[Service]
WorkingDirectory=/var/www/chatsupport
ExecStart=/usr/bin/dotnet /var/www/chatsupport/ChatSupport.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=chatsupport
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
# Service'i başlat
sudo systemctl daemon-reload
sudo systemctl enable chatsupport
sudo systemctl start chatsupport

# Durumu kontrol et
sudo systemctl status chatsupport

# Logları görüntüle
sudo journalctl -u chatsupport -f
```

#### 6. Nginx Yapılandırma

```bash
sudo nano /etc/nginx/sites-available/chatsupport
```

```nginx
server {
    listen 80;
    listen [::]:80;
    server_name yourdomain.com www.yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Real-IP $remote_addr;
        
        # SignalR için timeout ayarları
        proxy_read_timeout 300;
        proxy_connect_timeout 300;
        proxy_send_timeout 300;
    }
}
```

```bash
# Site'ı aktif et
sudo ln -s /etc/nginx/sites-available/chatsupport /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

#### 7. SSL Sertifikası (Let's Encrypt)

```bash
# Certbot yükle
sudo apt install -y certbot python3-certbot-nginx

# SSL sertifikası al
sudo certbot --nginx -d yourdomain.com -d www.yourdomain.com

# Otomatik yenileme testi
sudo certbot renew --dry-run
```

---

### C. Docker Deployment

#### 1. Dockerfile

Proje kök dizininde `Dockerfile` oluşturun:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["ChatSupport.csproj", "./"]
RUN dotnet restore "ChatSupport.csproj"
COPY . .
RUN dotnet build "ChatSupport.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChatSupport.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChatSupport.dll"]
```

#### 2. docker-compose.yml

```yaml
version: '3.8'

services:
  mysql:
    image: mysql:8.0
    container_name: chatsupport-mysql
    restart: always
    environment:
      MYSQL_ROOT_PASSWORD: rootpassword123
      MYSQL_DATABASE: ChatDb
      MYSQL_USER: chatuser
      MYSQL_PASSWORD: chatpassword123
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql
    networks:
      - chatsupport-network

  chatsupport:
    build: .
    container_name: chatsupport-app
    restart: always
    ports:
      - "5000:5000"
    depends_on:
      - mysql
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__MySqlConnection=Server=mysql;Port=3306;Database=ChatDb;Uid=chatuser;Pwd=chatpassword123;
    networks:
      - chatsupport-network

volumes:
  mysql_data:

networks:
  chatsupport-network:
    driver: bridge
```

#### 3. Docker Komutları

```bash
# Build ve başlat
docker-compose up -d

# Logları görüntüle
docker-compose logs -f chatsupport

# Durumu kontrol et
docker-compose ps

# Durdur ve kaldır
docker-compose down

# Verileri de silmek için
docker-compose down -v
```

---

## 🔒 Production Güvenlik Yapılandırması

### 1. Program.cs Güncellemeleri

```csharp
// Production için CORS politikası
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins(
            "https://yourdomain.com",
            "https://www.yourdomain.com"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// HTTPS redirect
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// CORS kullan
app.UseCors("Production");
```

### 2. Admin Authentication İyileştirme

Production'da `admin_login.html` yerine gerçek bir authentication sistemi kullanın:

- ASP.NET Core Identity
- JWT Token Authentication
- OAuth 2.0 / OpenID Connect
- Azure AD / Auth0

### 3. Rate Limiting

```bash
dotnet add package AspNetCoreRateLimit
```

```csharp
// Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };
});
```

### 4. Environment Variables

**Linux:**
```bash
export ConnectionStrings__MySqlConnection="Server=...;"
export AdminPassword="güçlü_şifre"
```

**Windows:**
```powershell
[Environment]::SetEnvironmentVariable("ConnectionStrings__MySqlConnection", "Server=...;", "Machine")
```

**Docker:**
```yaml
environment:
  - ConnectionStrings__MySqlConnection=Server=...
```

---

## 📊 Monitoring ve Logging

### Application Insights (Azure)

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Serilog

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/chatsupport-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

---

## ✅ Deployment Sonrası Kontroller

- [ ] Uygulama çalışıyor mu? (https://yourdomain.com)
- [ ] Admin paneline giriş yapılabiliyor mu?
- [ ] Chat widget çalışıyor mu?
- [ ] SignalR bağlantısı kuruluyor mu?
- [ ] Veritabanı bağlantısı çalışıyor mu?
- [ ] SSL sertifikası geçerli mi?
- [ ] CORS ayarları doğru mu?
- [ ] Loglar düzgün yazılıyor mu?
- [ ] Otomatik yeniden başlatma çalışıyor mu?

---

## 🐛 Sorun Giderme

### Uygulama başlamıyor

```bash
# Logları kontrol et
sudo journalctl -u chatsupport -n 100 --no-pager

# Manuel başlatma
cd /var/www/chatsupport
dotnet ChatSupport.dll
```

### Database bağlantı hatası

```bash
# MySQL çalışıyor mu?
sudo systemctl status mysql

# Bağlantı testi
mysql -u chatuser -p -h localhost ChatDb
```

### SignalR bağlanamıyor

- Firewall kontrolü
- Nginx timeout ayarları
- CORS policy kontrolü
- WebSocket desteği aktif mi?

---

## 🔄 Güncelleme Prosedürü

```bash
# 1. Yeni versiyonu publish et
dotnet publish -c Release -o ./publish-new

# 2. Backup al
sudo cp -r /var/www/chatsupport /var/www/chatsupport-backup

# 3. Service'i durdur
sudo systemctl stop chatsupport

# 4. Dosyaları güncelle
sudo rsync -av ./publish-new/ /var/www/chatsupport/

# 5. Migration'ları çalıştır (gerekirse)
cd /var/www/chatsupport
sudo -u www-data dotnet ef database update

# 6. Service'i başlat
sudo systemctl start chatsupport

# 7. Durumu kontrol et
sudo systemctl status chatsupport
```

---

## 📞 Destek

Deployment sırasında sorun yaşıyorsanız:

1. Logları kontrol edin
2. README.md'deki "Sorun Giderme" bölümüne bakın
3. GitHub Issues'da arama yapın
4. Yeni issue açın

---

**Başarılı deployment'lar dileriz! 🚀**

