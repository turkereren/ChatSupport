# MySQL Root Şifresini Sıfırlama Script'i
# Yeni şifre: root123

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  MySQL Root Şifre Sıfırlama" -ForegroundColor Cyan
Write-Host "  Yeni Şifre: root123" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan

# Adım 1: MySQL servisini durdur
Write-Host "[1/6] MySQL servisi durduruluyor..." -ForegroundColor Yellow
try {
    Stop-Service MySQL80 -Force -ErrorAction Stop
    Write-Host "      ✓ Servis durduruldu`n" -ForegroundColor Green
} catch {
    Write-Host "      ! Servis zaten durdurulmuş olabilir`n" -ForegroundColor Gray
}

Start-Sleep -Seconds 2

# Adım 2: Tüm mysql process'lerini kapat
Write-Host "[2/6] Çalışan MySQL process'leri kapatılıyor..." -ForegroundColor Yellow
Get-Process mysqld -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "      ✓ Process'ler temizlendi`n" -ForegroundColor Green

# Adım 3: Geçici init dosyası oluştur
Write-Host "[3/6] Geçici şifre sıfırlama dosyası oluşturuluyor..." -ForegroundColor Yellow
$initFile = "$env:TEMP\mysql-init.txt"
$initContent = @"
ALTER USER 'root'@'localhost' IDENTIFIED BY 'root123';
FLUSH PRIVILEGES;
"@
$initContent | Out-File -FilePath $initFile -Encoding ASCII -Force
Write-Host "      ✓ Dosya oluşturuldu: $initFile`n" -ForegroundColor Green

# Adım 4: MySQL'i init-file ile başlat
Write-Host "[4/6] MySQL init-file modu ile başlatılıyor..." -ForegroundColor Yellow
$mysqldPath = "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysqld.exe"
$process = Start-Process -FilePath $mysqldPath -ArgumentList "--init-file=`"$initFile`"" -PassThru -WindowStyle Hidden
Write-Host "      ✓ MySQL başlatıldı (PID: $($process.Id))`n" -ForegroundColor Green

# Adım 5: 15 saniye bekle (şifre değişikliğinin uygulanması için)
Write-Host "[5/6] Şifre değişikliği uygulanıyor (15 saniye)..." -ForegroundColor Yellow
for ($i = 15; $i -gt 0; $i--) {
    Write-Host "      $i saniye kaldı..." -ForegroundColor Gray
    Start-Sleep -Seconds 1
}
Write-Host "      ✓ Bekleme tamamlandı`n" -ForegroundColor Green

# Adım 6: MySQL'i durdur ve normal başlat
Write-Host "[6/6] MySQL normal moda alınıyor..." -ForegroundColor Yellow
Get-Process mysqld -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 3

# Geçici dosyayı sil
Remove-Item $initFile -Force -ErrorAction SilentlyContinue

# Normal servisi başlat
Start-Service MySQL80
Start-Sleep -Seconds 3
Write-Host "      ✓ MySQL normal modda çalışıyor`n" -ForegroundColor Green

# Adım 7: Test et
Write-Host "[TEST] Yeni şifre test ediliyor..." -ForegroundColor Yellow
$testResult = & "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -p"root123" -e "SELECT 'OK' AS Status;" 2>&1
if ($testResult -match "OK") {
    Write-Host "`n========================================" -ForegroundColor Green
    Write-Host "  ✓ BAŞARILI!" -ForegroundColor Green
    Write-Host "  MySQL Root Şifresi: root123" -ForegroundColor Yellow
    Write-Host "========================================`n" -ForegroundColor Green
} else {
    Write-Host "`n⚠ Test başarısız. Manuel kontrol gerekebilir.`n" -ForegroundColor Red
    Write-Host "Hata: $testResult`n" -ForegroundColor Red
}

Write-Host "Script tamamlandı. Bu pencereyi kapatabilirsiniz." -ForegroundColor Cyan


