# 🎓 HawkinsHS - Lise Yönetim Sistemi

Modern ve kapsamlı bir lise yönetim sistemi. Öğrenciler ders programlarını, sınav takvimini, notlarını ve duyuruları görüntüleyebilir. Öğretmenler ders, sınav, not ve duyuru yönetimi yapabilir. Admin kullanıcıları tüm sistem yönetimini gerçekleştirebilir.

## 🚀 Teknoloji Stack

- **.NET 8.0** - ASP.NET Core MVC
- **Entity Framework Core 8.0** - ORM & SQL Server
- **ASP.NET Core Identity** - Kimlik doğrulama ve yetkilendirme
- **AutoMapper** - DTO/ViewModel mapping
- **FluentValidation** - Model validasyonu
- **Serilog** - Loglama
- **Bootstrap 5** - UI Framework

## 📋 Özellikler

### Öğrenci Özellikleri
- ✅ Ders programını görüntüleme
- ✅ Sınav takvimini takip etme
- ✅ Notları görüntüleme
- ✅ Duyuruları okuma
- ✅ Devamsızlık takibi

### Öğretmen Özellikleri
- ✅ Ders yönetimi (CRUD)
- ✅ Sınav oluşturma ve düzenleme
- ✅ Not girişi
- ✅ Duyuru yayınlama
- ✅ Öğrenci listesi görüntüleme
- ✅ Ders programı yönetimi

### Admin Özellikleri
- ✅ Kullanıcı yönetimi
- ✅ Öğrenci ve öğretmen yönetimi
- ✅ Tüm dersleri yönetme
- ✅ Sistem raporları
- ✅ Duyuru yönetimi

## 🗂️ Proje Yapısı

```
HawkinsHS/
├── Controllers/        # MVC Controllers
├── Data/              # DbContext, Migrations, SeedData
├── Models/            # Domain Models
├── ViewModels/        # View Models
├── Views/             # Razor Views
├── Services/          # Business Logic Services
├── Validators/        # FluentValidation Rules
├── Mappings/          # AutoMapper Profiles
└── wwwroot/           # Static Files
```

## 🛠️ Kurulum

### Gereksinimler

- .NET 8.0 SDK
- SQL Server (LocalDB veya Express)
- Visual Studio 2022 / VS Code

### Adım Adım Kurulum

1. **Projeyi klonlayın:**
```bash
git clone <repo-url>
cd Hawkins-HS
```

2. **NuGet paketlerini yükleyin:**
```bash
cd Hawkins-HS
dotnet restore
```

3. **Veritabanını oluşturun:**
```bash
dotnet ef database update
```

Veritabanı otomatik olarak seed data ile doldurulacaktır.

4. **Projeyi çalıştırın:**
```bash
dotnet run
```

5. **Tarayıcıda açın:**
```
https://localhost:5001
```

## 👥 Demo Hesaplar

Sistem otomatik olarak aşağıdaki demo hesapları oluşturur:

### Admin
- **Kullanıcı Adı:** admin
- **Şifre:** P@ssw0rd!
- **Yetki:** Tam sistem yönetimi

### Öğretmenler
- **Kullanıcı Adı:** e.watson
- **Şifre:** Teacher@123
- **Bölüm:** Matematik

### Öğrenciler
- **Kullanıcı Adı:** lucas
- **Şifre:** Student@123
- **Sınıf:** 11-A

*Daha fazla demo hesap için seed data'ya bakınız.*

## 📊 Veritabanı Şeması

### Ana Tablolar

- **ApplicationUser** - Identity kullanıcıları (Admin, Teacher, Student)
- **Student** - Öğrenci bilgileri
- **Teacher** - Öğretmen bilgileri
- **Course** - Ders bilgileri
- **Enrollment** - Öğrenci-Ders ilişkisi
- **ClassSchedule** - Haftalık ders programı
- **Exam** - Sınav bilgileri
- **Grade** - Sınav notları
- **Announcement** - Duyurular
- **Attendance** - Devamsızlık kayıtları

## 🔧 Yapılandırma

### Connection String

`appsettings.json` dosyasında veritabanı bağlantı dizesini düzenleyin:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HawkinsHS;Trusted_Connection=true"
  }
}
```

### Serilog

Log dosyaları `Logs/` klasöründe oluşturulur. Yapılandırma `appsettings.json` içinde bulunur.

## 📝 Migration Komutları

Yeni migration eklemek için:
```bash
dotnet ef migrations add MigrationName
```

Veritabanını güncellemek için:
```bash
dotnet ef database update
```

Son migration'ı geri almak için:
```bash
dotnet ef migrations remove
```

## 🧪 Test

(Test projesi oluşturulacak)

```bash
dotnet test
```

## 📈 Geliştirme Roadmap

### ✅ Tamamlanan
- [x] Proje yapısı ve temel setup
- [x] Identity authentication/authorization
- [x] Domain modelleri ve DbContext
- [x] Seed data ve demo hesaplar
- [x] Temel CRUD operasyonları
- [x] Login/Logout işlemleri
- [x] Role-based dashboard

### 🔄 Devam Eden
- [ ] Controllers (Students, Teachers, Courses, Exams, Grades, Announcements)
- [ ] CRUD Views
- [ ] Calendar integration (FullCalendar)
- [ ] Dashboard views (Admin, Teacher, Student)
- [ ] AutoMapper profiles
- [ ] FluentValidation rules

### 📅 Planlanan
- [ ] Notification service (Email)
- [ ] Background job service (Exam reminders)
- [ ] PDF export (Reports, Grade cards)
- [ ] Unit tests
- [ ] API endpoints
- [ ] Docker support

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request açın

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır.

## 📧 İletişim

Proje Sahibi - [Your Name]

Proje Link: [https://github.com/yourusername/hawkinshs](https://github.com/yourusername/hawkinshs)

---

**Not:** Bu proje eğitim amaçlıdır ve production kullanımı için ek güvenlik önlemleri alınmalıdır.
