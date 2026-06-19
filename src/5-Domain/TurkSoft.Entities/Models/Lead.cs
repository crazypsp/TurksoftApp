using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================
// DOMAIN MODEL: Lead (Müşteri Adayı / Potansiyel Müşteri)
// ============================================================================
// 
// AMAÇ (Purpose):
//   Lead sınıfı, CRM sisteminin temel domain modelidir. Bir potansiyel müşteri
//   (lead) hakkında tüm verileri içerir. Bu sınıf tamamen iş kurallarına
//   odaklanır ve veri tabanı yapısıyla doğrudan ilişkilidir.
//
// SORUMLULUKLAR (Responsibilities):
//   1. Lead'in tüm özelliklerini tanımlamak (ad, email, telefon, vb.)
//   2. Lead'in yaşam döngüsünü takip etmek (durum, aşama, vb.)
//   3. Lead ile ilgili ilişkili verileri tutmak (aktiviteler, notlar, vb.)
//   4. Veri doğrulama kurallarını tanımlamak (validation attributes)
//   5. EF Core mapping için gerekli yapıyı sağlamak
//
// TASARIM PATTERNİ (Design Pattern):
//   - Entity Pattern: Domain-Driven Design (DDD) prensipleriyle oluşturulmuş
//   - Aggregate Root: Lead kendisi ve ilişkili veriler bu aggregate root içinde
//   - Value Object: Bazı özellikler (Address, ContactInfo) value object olabilir
//
// BEST PRACTICES:
//   ✓ Yapılandırıcı (Constructor) ile default değerler atanır
//   ✓ Parametersiz yapılandırıcı EF Core'un hibernasyonu için gereklidir
//   ✓ Navigation properties için virtual anahtar kelimesi kullanılır
//   ✓ Veri doğrulama [Required], [StringLength], vb. attributes ile yapılır
//   ✓ Timestamp alanları (CreatedAt, UpdatedAt) audit trail için tutulur
//
// PERFORMANCE NOTLARI:
//   - Lazy loading için virtual navigation properties kullanılır
//   - Eager loading istenir ise .Include() ile yapılır
//   - Shadow properties EF Core'da ilişkiler için otomatik oluşturulur
//   - Computed properties mapping'de [NotMapped] ile işaretlenir
//
// SECURITY NOTLARI:
//   - Hassas veriler (phone, email) encryptable yapıya uygun tasarlanmış
//   - Soft delete yapısı için IsDeleted boolean'ı eklenebilir
//   - Audit properties (CreatedBy, ModifiedBy) eklenebilir
//
// ============================================================================

namespace TurkSoft.Entities.Models
{
    /// <summary>
    /// Lead (Potansiyel Müşteri) Entity Sınıfı
    /// Müşteri adaylarının tüm bilgilerini temsil eder.
    /// </summary>
    [Table("Leads", Schema = "CRM")]
    // TABLE ATRIBUTU:
    //   - "Leads": Veritabanındaki tablo adı
    //   - Schema = "CRM": SQL Server'da şema adı (logical grouping)
    //   - Avantaj: Aynı veritabanında çok sayıda şema ile organize edebiliriz
    //   - Performance: Şema sayesinde sorgu planlayıcı daha hızlı çalışır
    public class Lead
    {
        // ====================================================================
        // PRIMARY KEY VE TEMEL KİMLİK BİLGİSİ
        // ====================================================================

        /// <summary>
        /// Lead'in benzersiz tanımlayıcısı (Primary Key)
        /// </summary>
        [Key]
        // KEY ATTRIBUTE:
        //   - EF Core'a bunun Primary Key olduğunu söyler
        //   - SQL: CREATE TABLE Leads (Id INT PRIMARY KEY IDENTITY(1,1))
        //   - Performance: Veritabanı otomatik olarak index oluşturur
        public int Id { get; set; }

        /// <summary>
        /// Lead'in adı (İşletme adı veya kişi adı)
        /// </summary>
        [Required(ErrorMessage = "Lead adı boş bırakılamaz.")]
        // REQUIRED ATTRIBUTE:
        //   - Validation: Model state'de null/empty kontrolü yapılır
        //   - SQL: NOT NULL kısıtlaması eklenir
        //   - Avantaj: Database consistency sağlanır
        //   - Client-side de HTML5 validation çalışır
        [StringLength(200, MinimumLength = 2, 
            ErrorMessage = "Lead adı 2-200 karakter arasında olmalıdır.")]
        // STRING LENGTH ATTRIBUTE:
        //   - MaxLength: 200 karakter limiti (database varchar(200))
        //   - MinimumLength: En az 2 karakter gerekli
        //   - Performance: Veri tabanında 200 karakterlik alan ayrılır
        //   - SQL: VARCHAR(200) NOT NULL
        //   - Security: SQL injection ve buffer overflow'dan korur
        public string Name { get; set; } = string.Empty;
        // DEFAULT VALUE:
        //   - string.Empty atanması null reference exception'ı önler
        //   - C# 8.0+ nullable reference types ile uyumlu
        //   - Avantaj: Null pointer exception riskini azaltır

        // ====================================================================
        // İLETİŞİM BİLGİLERİ
        // ====================================================================

        /// <summary>
        /// Lead'in email adresi (İletişim aracı)
        /// </summary>
        [Required(ErrorMessage = "Email adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi girin.")]
        // EMAIL ADDRESS ATTRIBUTE:
        //   - Regex pattern: ^[^@\s]+@[^@\s]+\.[^@\s]+$
        //   - Validation: Server-side email format kontrolü
        //   - Client-side: HTML5 <input type="email"/> ile çalışır
        //   - Security: Email injection attack'larından kısmen korur
        //   - Not: RFC 5322 standardının tam uyumluluğu garanti etmez
        [StringLength(255)]
        // WHY 255 CHARS?
        //   - RFC 5321: Email maksimum 254 karakter olabilir
        //   - 255 karakter: UTF-8 encoded maksimum uzunluk + buffer
        //   - SQL: VARCHAR(255) standart internet email uzunluğu
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Lead'in telefon numarası (İletişim aracı)
        /// </summary>
        [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
        // PHONE ATTRIBUTE:
        //   - Regex pattern: ^[\d\s\-\+\(\)]+$
        //   - Validation: Telefon numarası formatı kontrolü
        //   - Avantaj: Uluslararası formatları destekler
        //   - Not: Kesin formatı veritabanında kontrol edilmeli
        [StringLength(20)]
        // WHY 20 CHARS?
        //   - E.164 format: +[country code][number] = max 15 char
        //   - 20 char: Formatting characters (+, -, (, )) için buffer
        //   - SQL: VARCHAR(20) yeterli
        public string? Phone { get; set; };
        // NULLABLE (?):
        //   - Telefon numarası opsiyoneldir (null olabilir)
        //   - SQL: VARCHAR(20) NULL
        //   - Avantaj: Her lead'in telefonu olmayabilir

        /// <summary>
        /// Lead'in şirket adı (Bağlamsal bilgi)
        /// </summary>
        [StringLength(300)]
        public string? CompanyName { get; set; };

        // ====================================================================
        // LEAD DURUMU VE KATEGORİZASYON (Status & Classification)
        // ====================================================================

        /// <summary>
        /// Lead'in mevcut durumu (Açık, Kapalı, İşlemde, vb.)
        /// Enum kullanılması string'e göre avantajlıdır
        /// </summary>
        [Required]
        [EnumDataType(typeof(LeadStatus), 
            ErrorMessage = "Geçerli bir lead durumu seçiniz.")]
        // ENUM DATA TYPE ATTRIBUTE:
        //   - Validation: Sadece tanımlı enum değerleri kabul edilir
        //   - Security: Invalid enum değerleri reject edilir
        //   - Performance: Database'de integer saklanır (1, 2, 3 vs)
        //   - Avantaj: Type-safe, compile-time kontrol
        public LeadStatus Status { get; set; } = LeadStatus.New;
        // DEFAULT VALUE: LeadStatus.New
        //   - Yeni oluşturulan lead'ler New (Yeni) durumunda başlar
        //   - İş kuralı: Satış sürecinin başlangıç aşaması
        //   - SQL: TINYINT (1 byte) saklanır

        /// <summary>
        /// Lead'in satış aşaması (Prospecting, Qualification, vb.)
        /// Customer journey'nin hangi aşamasında olduğunu belirtir
        /// </summary>
        [Required]
        [EnumDataType(typeof(LeadStage))]
        public LeadStage Stage { get; set; } = LeadStage.Prospecting;
        // LEAD STAGE:
        //   - Prospecting: Potansiyel müşteri aranması
        //   - Qualification: Müşteri uygunluğu kontrol
        //   - Negotiation: Fiyat ve şartlar görüşülüyor
        //   - Closing: Son anlaşmalar yapılıyor
        //   - Won: Başarıyla kapandı
        //   - Lost: Kaybedildi

        /// <summary>
        /// Lead'in öncelik seviyesi (Düşük, Orta, Yüksek)
        /// Satış takımının çalışma sıralamasını belirler
        /// </summary>
        [Range(1, 5, ErrorMessage = "Öncelik 1-5 arasında olmalıdır.")]
        // RANGE ATTRIBUTE:
        //   - Minimum: 1 (En düşük priorite)
        //   - Maximum: 5 (En yüksek priorite)
        //   - Validation: Sadece 1-5 arası değerler kabul edilir
        //   - Avantaj: Database constraint olarak da eklenebilir
        public int Priority { get; set; } = 3; // Default: Orta (3)
        // SKALA:
        //   - 1-2: Düşük (Follow-up'a gerek yok)
        //   - 3: Orta (Normal satış süreci)
        //   - 4-5: Yüksek (Acil takip gerekli)

        // ====================================================================
        // KAYNAK BİLGİSİ (Source Information)
        // ====================================================================

        /// <summary>
        /// Lead'in nereden geldiği (Web sitesi, Referral, Reklam, vb.)
        /// Marketing attribution ve ROI hesabı için önemli
        /// </summary>
        [EnumDataType(typeof(LeadSource))]
        // LEAD SOURCE TRACKING:
        //   - Marketing ROI hesabı: Hangi kanal en iyi lead veriyor?
        //   - Attribution: Satışın hangi marketing aktivitesinden geldiği
        //   - Performance metrik: Channel efficiency analysis
        //   - SQL: TINYINT olarak saklanır (0-256 arası)
        public LeadSource Source { get; set; } = LeadSource.WebSite;
        // DEFAULT: WebSite
        //   - Çoğu lead web sitesinden gelir
        //   - Fallback value olarak kullanılır

        /// <summary>
        /// Lead'in nereden geldiğinin detaylı açıklaması
        /// Örneğin: Google Ads kampanyası, Linkedln bağlantısı, vb.
        /// </summary>
        [StringLength(500)]
        public string? SourceDetail { get; set; };
        // AMAÇ:
        //   - Lead source'un daha detaylı bilgisini tutar
        //   - Örnek: "Google Ads - CRM Aracı"
        //   - Marketing team'e granular reporting imkanı verir

        // ====================================================================
        // COĞRAFİ BİLGİLER (Geographic Information)
        // ====================================================================

        /// <summary>
        /// Lead'in bulunduğu ülke (İş kanunları, dil, para birimi için önemli)
        /// </summary>
        [StringLength(100)]
        public string? Country { get; set; };
        // AMAÇ:
        //   - Localization: Dil ve para birimi seçimi
        //   - Compliance: Ülkeye özgü yasal gereklilikler
        //   - Sales territory: Satış bölgesi atama
        //   - Performance: Region-based reporting

        /// <summary>
        /// Lead'in bulunduğu şehir (Lojistik ve müşteri hizmetleri için)
        /// </summary>
        [StringLength(100)]
        public string? City { get; set; };

        /// <summary>
        /// Lead'in bulunduğu sektör/endüstri (Ürün uyumluluğu için)
        /// </summary>
        [EnumDataType(typeof(Industry))]
        public Industry? Industry { get; set; };
        // NULLABLE ENUM:
        //   - Bazı lead'ler sektör bilgisi olmadan gelebilir
        //   - Qualification sırasında doldurulabilir
        //   - Filtreleme ve segmentasyon için kullanılır

        // ====================================================================
        // FİNANSAL BİLGİLER (Financial Information)
        // ====================================================================

        /// <summary>
        /// Lead'in tahmini bütçesi (Satış olasılığını belirler)
        /// Null = Bilinmiyor, 0 = Bütçe yok
        /// </summary>
        [Range(0, 999999999, ErrorMessage = "Bütçe 0 ile 999,999,999 arasında olmalıdır.")]
        [Column(TypeName = "decimal(18,2)")]
        // COLUMN TYPE NAME:
        //   - decimal(18,2): 18 hane, 2 ondalık basamak
        //   - Örnek: 1,234,567,890.12
        //   - Avantaj: Para birimi hesaplamalarında kesinlik
        //   - SQL: DECIMAL(18,2) NOT NULL DEFAULT 0
        //   - Neden decimal, double değil?
        //     * Floating point arithmetic'te rounding error var
        //     * Decimal binary-coded decimal kullanır = exact
        //     * Para için decimal güvenli seçimdir
        public decimal? EstimatedBudget { get; set; };
        // NULLABLE DECIMAL:
        //   - Lead'in bütçesi bilinmeyebilir
        //   - Null = Bilgi henüz alınmadı
        //   - 0 = Bütçe sınırı yok

        /// <summary>
        /// Lead'den tahmini gelir (Annual Contract Value)
        /// Satış forecast ve revenue projection için
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? ExpectedValue { get; set; };
        // AMAÇ:
        //   - Revenue forecast: "Bu çeyrek tarafından ne kadar gelir olacak?"
        //   - Deal size: Lead'in ortalama contract value'su
        //   - Performance: Team'in hedef gerçekleştirme durumu
        //   - Örnek: 50,000 TL contract değeri = 50000.00

        // ====================================================================
        /// <summary>
        /// Lead'in kapalı olma tarihi (Ne zaman para alacağımız bekleniyor?)
        /// Null = Henüz bilinmiyor
        /// </summary>
        [DataType(DataType.Date)]
        public DateTime? ExpectedCloseDate { get; set; };
        // DATA TYPE DATE:
        //   - SQL: DATE (sadece tarih, saat yok)
        //   - Avantaj: Database'de daha az yer kaplar
        //   - Format: YYYY-MM-DD
        //   - Amaç: İş takvimi planlaması, forecast

        // ====================================================================
        // AUDIT VE TIMESTAMP BİLGİLERİ (Audit Trail)
        // ====================================================================

        /// <summary>
        /// Lead'in ilk oluşturulma tarihi (Immutable - hiçbir zaman değişmez)
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // UTC NOW:
        //   - UTC (Koordinatlandırılmış Evrensel Zaman) kullanılır
        //   - Avantaj: Zaman dilimi sorunları ortadan kalkar
        //   - Best Practice: Veritabanında her zaman UTC sakla
        //   - Presentation'da client'in zaman dilimine çevir
        //   - SQL: DATETIME2 NOT NULL DEFAULT GETUTCDATE()
        // IMMUTABLE:
        //   - Oluşturulduktan sonra asla değiştirilmez
        //   - Who created this record?
        //   - When was this created?
        //   - Audit trail için kritik

        /// <summary>
        /// Lead'in son güncellenme tarihi (İzlenebilirlik için)
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; };
        // NULLABLE DATETIME:
        //   - Null = Henüz güncellenmemiş, sadece create'i var
        //   - Değer varsa = Son değiştirilme zamanı
        //   - Avantaj: "Kaç gün takip yapılmadı?" sorusuna cevap verir

        /// <summary>
        /// Lead'i oluşturan kullanıcının ID'si (Accountability)
        /// </summary>
        [Required]
        [StringLength(128)]
        public string CreatedBy { get; set; } = "System";
        // CREATED BY:
        //   - Hangi kullanıcı bu lead'i oluşturdu?
        //   - Accountability: Kim neye karar verdi?
        //   - Tracking: Performance metrikleri (Kim en iyi lead veriyor?)
        //   - GDPR compliance: Data lineage
        //   - Default: "System" (automated process)
        //   - SQL: VARCHAR(128) - Genelde User ID veya Email

        /// <summary>
        /// Lead'i son güncelleyen kullanıcının ID'si
        /// </summary>
        [StringLength(128)]
        public string? UpdatedBy { get; set; };
        // UPDATED BY:
        //   - Null = Henüz kimse tarafından güncellenmemiş
        //   - Who last modified this record?
        //   - When did they modify it?
        //   - Audit log: Değişiklik geçmişini izleme

        // ====================================================================
        // İLİŞKİLER (Relationships / Navigation Properties)
        // ====================================================================

        /// <summary>
        /// Bu Lead'e ait tüm Aktiviteler (Aramalar, Emailler, Toplantılar)
        /// One-to-Many relationship
        /// </summary>
        [NotMapped]
        // NOT MAPPED:
        //   - EF Core bunu foreign key olarak veritabanında saklamaz
        //   - Sadece navigation property (ilişki navigate etmek için)
        //   - SQL'de Activities tablosundaki LeadId ile ilişkilidir
        //   - Avantaj: O/RM (Object-Relational Mapping) kullanabiliriz
        public virtual ICollection<Activity> Activities { get; set; } 
            = new List<Activity>();
        // VIRTUAL ANAHTAR KELIMESI:
        //   - EF Core lazy loading için gerekli
        //   - Runtime'da proxy oluşturulabilir
        //   - Avantaj: Sadece ihtiyaç duyulduğunda verileri yükler
        //   - Dezavantaj: N+1 query problem'i oluşabilir
        //   - Çözüm: Eager loading (.Include()) kullanılabilir
        // INITIALIZATION:
        //   - new List<Activity>() = Default olarak boş collection
        //   - Null reference exception'ı önler
        //   - Add, Remove işlemleri güvenli olur

        /// <summary>
        /// Bu Lead'e ait tüm Notlar (Internal comments ve timeline)
        /// </summary>
        public virtual ICollection<Note> Notes { get; set; } 
            = new List<Note>();
        // AMAÇ:
        //   - Internal notes: Takım üyeleri arasında iletişim
        //   - Timeline: Lead'in tüm etkileşimleri görmek
        //   - Collaboration: Kimin ne yazını görmek
        //   - Örnek notlar:
        //     * "Müşteri pazartesi çağıracak"
        //     * "Fiyat konusunda %10 indirim talep etti"
        //     * "Teknik şartnameler gönderildi"

        /// <summary>
        /// Lead'e atanmış satış temsilcisi (Sales Rep)
        /// Foreign Key: AssignedToUserId
        /// </summary>
        [ForeignKey(nameof(AssignedToUserId))]
        // FOREIGN KEY ATTRIBUTE:
        //   - Navigation property ile foreign key'i eşleştirir
        //   - SQL: Foreign Key constraint oluşturulur
        //   - Referential integrity: Yanlış user ID atanmaz
        //   - Avantaj: Database consistency sağlanır
        public virtual User? AssignedToUser { get; set; };
        // NULLABLE NAVIGATION:
        //   - Lead'e henüz kullanıcı atanmamış olabilir
        //   - Null = Henüz kimse sorumlu değil
        //   - Workflow: Lead oluşturulduktan sonra atanabilir

        /// <summary>
        /// Lead'e atanmış kullanıcının ID'si (Foreign Key)
        /// </summary>
        public string? AssignedToUserId { get; set; };
        // FOREIGN KEY:
        //   - AssignedToUser navigation property'si ile ilişkili
        //   - SQL: INT (User tablosunun primary key'i)
        //   - Null: Lead'e kullanıcı atanmamışsa null
        //   - Constraint: Users.Id'ye referans

        // ====================================================================
        // EXTENSION PROPERTIES (Genişletilebilirlik için)
        // ====================================================================

        /// <summary>
        /// Lead'in özel alanları (Custom fields - JSON formatında)
        /// Veri tabanı şema değişikliği yapılmadan yeni alanlar eklenebilir
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        // NVARCHAR(MAX):
        //   - SQL Server: Maksimum 2GB text veri
        //   - Unicode support: Türkçe, Çince, Japonca vs destekler
        //   - EF Core: JSON serialize/deserialize edilir
        //   - Performance: İndeks oluşturulmaz, full-text search zor
        //   - Avantaj: Schema flexibility (Schemaless veri tutar)
        public string? CustomFields { get; set; };
        // CUSTOM FIELDS PATTERN:
        //   - "Schema flexibility": Yeni alanlar eklemek kolay
        //   - Migration gereksiz: Database versioning sorunu yok
        //   - Dezavantaj: Type safety yok, query etmek zor
        //   - Örnek JSON:
        //     {
        //       "referralCode": "ABC123",
        //       "industrySubType": "Manufacturing",
        //       "projectDuration": "6 months"
        //     }
        //   - Kullanım: Uygulamada 80/20 kuralı - nadiren kullanılan alanlar

        // ====================================================================
        // COMPUTED PROPERTIES (Hesaplanmış Özellikler)
        // ====================================================================

        /// <summary>
        /// Lead'in kaç gün takip yapılmadığını hesaplar
        /// (Sadece hesaplama, veritabanında saklanmaz)
        /// </summary>
        [NotMapped]
        public int DaysSinceLastUpdate
        {
            get
            {
                // COMPUTED PROPERTY:
                //   - [NotMapped]: Veritabanında saklanmaz
                //   - Runtime'da hesaplanır
                //   - Avantaj: Data redundancy yok
                //   - Dezavantaj: Her erişimde hesaplanır (caching gerekli)
                var lastUpdate = UpdatedAt ?? CreatedAt;
                // COALESCING (UpdatedAt ?? CreatedAt):
                //   - UpdatedAt null ise CreatedAt kullan
                //   - Lead hiç güncellenmemişse CreatedAt'ı al
                //   - Garantiler: Her zaman valid bir tarih döner
                return (int)(DateTime.UtcNow - lastUpdate).TotalDays;
                // TOTAL DAYS:
                //   - TimeSpan.TotalDays: Double olarak gün sayısını döner
                //   - (int) cast: Integer'a çevrilir (0.5 gün = 0 gün)
                //   - Amaç: "Kaç gün takip yapılmadı?" metriki
                //   - Örnek: 5 gün takip yapılmamışsa = 5 döner
            }
        }

        /// <summary>
        /// Lead'in satış olasılığını yüzde olarak hesaplar
        /// Duruma, aşamaya ve diğer faktörlere göre
        /// </summary>
        [NotMapped]
        public int SalesWinProbability
        {
            get
            {
                // WIN PROBABILITY CALCULATION:
                //   - Machine learning yerine business rule kullanılır
                //   - Base probability: Stage'ye göre başlangıç değeri
                //   - Modifiers: Priority, Budget, Expected close date
                //   - Result: 0-100 yüzde

                var baseProbability = Stage switch
                {
                    // SWITCH EXPRESSION (C# 8.0+):
                    //   - Daha clean if-else yerine
                    //   - Pattern matching: Her duruma olasılık değeri
                    //   - Performance: Match statement'tan hızlı
                    LeadStage.Prospecting => 10,      // Yeni araştırma aşaması: %10
                    LeadStage.Qualification => 20,    // Nitelik kontrol: %20
                    LeadStage.Proposal => 40,         // Teklif gönderildi: %40
                    LeadStage.Negotiation => 60,      // Fiyat görüşülüyor: %60
                    LeadStage.Closing => 80,          // Son aşama: %80
                    LeadStage.Won => 100,             // Kazanıldı: %100
                    LeadStage.Lost => 0,              // Kaybedildi: %0
                    _ => 10                           // Default: %10
                    // _ => 10: Pattern matching'in fallback
                    //   - Tanımlanmamış enum değerleri için
                    //   - Safety: Compiler uyarı vermez
                };

                // PRIORITY MODIFIER:
                //   - Yüksek priority lead'ler daha çok takip alır
                //   - Win probability'si daha yüksektir
                var priorityBonus = Priority >= 4 ? 10 : 0;
                // Ternary operator: if (Priority >= 4) ? 10 : 0

                // BUDGET MODIFIER:
                //   - Bütçesi olan lead'lerin win probability'si yüksektir
                var budgetBonus = EstimatedBudget.HasValue && 
                                  EstimatedBudget.Value > 0 ? 5 : 0;
                // HASVALUE: Nullable type'ın değer içerip içermediğini kontrol

                // CLOSE DATE MODIFIER:
                //   - Yakın tarihli close date = Daha ciddi lead
                var closeDate = ExpectedCloseDate ?? DateTime.MaxValue;
                var closeBonus = closeDate < DateTime.UtcNow.AddDays(30) ? 10 : 0;
                // AddDays(30): 30 gün içinde kapanması beklenen lead
                //   - Urgency factor: Satış zaten ileri aşamada
                //   - Close date'in validity'si önemli

                // TOTAL CALCULATION:
                //   - Tüm bonusları topla ve max 100'e cap et
                var totalProbability = Math.Min(baseProbability + 
                    priorityBonus + budgetBonus + closeBonus, 100);
                // Math.Min():
                //   - İki değerden küçüğünü seçer
                //   - Probability hiçbir zaman 100'ü geçemez
                //   - Avantaj: Business rule'e uygun

                return totalProbability;
            }
        }

        // ====================================================================
        // BUSINESS METHODS (İş Kuralları)
        // ====================================================================

        /// <summary>
        /// Lead'i başka bir aşamaya taşır (State transition)
        /// </summary>
        /// <param name="newStage">Yeni aşama</param>
        /// <returns>Başarılı olup olmadığı</returns>
        public bool MoveToStage(LeadStage newStage)
        {
            // STATE TRANSITION VALIDATION:
            //   - Lead'in geçebileceği durumların kuralları var
            //   - Örnek: Lost durumdan Negotiation'a geçemez
            //   - Avantaj: Business logic entity'de merkezi

            // INVALID TRANSITIONS:
            //   - Won/Lost'dan başka durumlara geçilemez
            //   - Prospecting'e geri dönülemez (sadece ileri)
            if ((Status == LeadStatus.Won || Status == LeadStatus.Lost) &&
                (newStage != LeadStage.Won && newStage != LeadStage.Lost))
            {
                return false; // Transition reddedildi
            }

            // BACKWARD MOVEMENT RESTRICTION:
            //   - Geriye dönüş izin verilmez (Negotiation -> Qualification)
            if ((int)newStage < (int)Stage && newStage != LeadStage.Prospecting)
            {
                return false; // Geriye dönüş reddedildi
            }

            // VALID TRANSITION:
            //   - Kurallara uygun geçiş
            Stage = newStage;
            UpdatedAt = DateTime.UtcNow;
            return true;
        }

        /// <summary>
        /// Lead'i kullanıcıya atar
        /// </summary>
        public void AssignToUser(string userId)
        {
            // ASSIGNMENT LOGIC:
            //   - Lead'i belirlenen kullanıcıya ata
            //   - Audit trail güncellenecek
            AssignedToUserId = userId;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Lead'in "warm" olup olmadığını kontrol eder
        /// (Yakın zamanda güncellenmişse warm, değilse cold)
        /// </summary>
        public bool IsWarm()
        {
            // LEAD TEMPERATURE:
            //   - Warm lead: 7 gün içinde güncellenmişse
            //   - Cold lead: 7 günden fazla takip yapılmamışsa
            //   - Amaç: Hangilerine acil takip yapılacağını bilmek
            var lastUpdate = UpdatedAt ?? CreatedAt;
            var daysSinceUpdate = (int)(DateTime.UtcNow - lastUpdate).TotalDays;
            return daysSinceUpdate <= 7; // 7 günden daha eski değil
        }
    }

    // ========================================================================
    // ENUM TANIMALARI (Enumeration Definitions)
    // ========================================================================

    /// <summary>
    /// Lead'in mevcut durumu
    /// </summary>
    public enum LeadStatus
    {
        // ENUMERATION:
        //   - Strongly typed values
        //   - Database'de integer (0, 1, 2, 3) saklanır
        //   - Avantaj: Type-safe, compile-time kontrol
        //   - Performance: String yerine integer daha hızlı

        /// <summary>Yeni lead, henüz nitelenmemiş</summary>
        New = 0,
        // DEĞER: 0 (Default değer, sıfır):
        //   - Yeni oluşturulan lead'ler bu durumda
        //   - SQL: 0 olarak saklanır
        //   - Avantaj: Default enum value'su

        /// <summary>Lead nitelendi, potansiyel müşteri</summary>
        Qualified = 1,
        // NitelenMiş lead'ler hakkında daha fazla bilgi var

        /// <summary>Lead aktif olarak çalışılıyor</summary>
        InProgress = 2,
        // Satış temsilcisi aktif takip yapıyor

        /// <summary>Lead işlemi tamamlandı</summary>
        Completed = 3
        // Satış kapandı veya lead kaybedildi
    }

    /// <summary>
    /// Lead'in satış funnel'ında bulunduğu aşama
    /// </summary>
    public enum LeadStage
    {
        // SALES FUNNEL STAGES:
        //   - Müşteri journey'nin her aşaması
        //   - Linear progression: Prospecting -> Won/Lost
        //   - Each stage: Farklı actions ve responsibilities

        Prospecting = 0,      // Potansiyel müşteri araştırması
        Qualification = 1,    // Müşteri uygunluğu kontrol
        Proposal = 2,         // Teklif hazırlanması
        Negotiation = 3,      // Fiyat ve şartlar görüşülüyor
        Closing = 4,          // Son anlaşmalar
        Won = 5,              // Başarıyla kapandı (Deal won)
        Lost = 6              // Kaybedildi (Lost opportunity)
    }

    /// <summary>
    /// Lead'in nereden geldiği (Marketing attribution)
    /// </summary>
    public enum LeadSource
    {
        // MARKETING ATTRIBUTION:
        //   - ROI hesabı: Hangi channel best performer?
        //   - Budget allocation: Hangi marketing'e daha çok para harcan?
        //   - Performance analysis: Channel efficiency

        WebSite = 0,          // Web sitesi (Organic/Paid)
        SearchEngine = 1,     // Google, Bing (SEO/SEM)
        SocialMedia = 2,      // Facebook, LinkedIn, Twitter
        EmailCampaign = 3,    // Email marketing
        Referral = 4,         // Müşteri referansı
        TradeShow = 5,        // Ticari fuar/etkinlik
        DirectCall = 6,       // Direkt arama (Cold calling)
        Partner = 7,          // İş ortağı referansı
        Advertisement = 8,    // Reklam (TV, Radio, Billboard)
        Other = 9             // Diğer
    }

    /// <summary>
    /// Lead'in sektörü/endüstrisi
    /// </summary>
    public enum Industry
    {
        // INDUSTRY CLASSIFICATION:
        //   - Product-market fit analizi
        //   - Vertical market strategy
        //   - Pricing by industry
        //   - Competitive analysis

        Technology = 0,
        Manufacturing = 1,
        Healthcare = 2,
        Finance = 3,
        Retail = 4,
        Education = 5,
        Logistics = 6,
        Energy = 7,
        Construction = 8,
        Agriculture = 9
    }
}
