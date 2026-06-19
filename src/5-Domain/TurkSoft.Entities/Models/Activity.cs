using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================
// DOMAIN MODEL: Activity (Aktivite - CRM Etkinliği)
// ============================================================================
// 
// AMAÇ (Purpose):
//   Activity sınıfı, bir Lead'le yapılan tüm etkileşimleri (aramalar,
//   emailler, toplantılar, görevler vb.) temsil eder. CRM'in kalbi budur:
//   "Ne yapılmıştır?" sorusunun cevabı.
//
// SORUMLULUKLAR (Responsibilities):
//   1. Etkileşimin türünü belirtmek (Call, Email, Meeting, Task)
//   2. Etkileşimin detaylarını tutmak (başlık, açıklama, katılımcılar)
//   3. Etkileşimin sonucunu kaydetmek (başarılı, başarısız vb.)
//   4. Etkileşimin zamanlamasını tutmak (planlanmış, tamamlanmış)
//   5. Lead'le Activity arasında ilişki kurmak
//
// PERFORMANCE NOTLARI:
//   - Activity'ler çok sayıda olabilir (Lead başına 100+)
//   - Filtreleme: Tarih, tür, sonuç bazında
//   - İndeks: ScheduledAt, CompletedAt, Type üzerine indexes
//   - Paging: Listeleme sırasında mutlaka paging kullan
//
// ============================================================================

namespace TurkSoft.Entities.Models
{
    /// <summary>
    /// Activity (Aktivite) Entity - Lead ile yapılan etkileşim
    /// </summary>
    [Table("Activities", Schema = "CRM")]
    public class Activity
    {
        // ====================================================================
        // PRIMARY KEY VE TEMEL KİMLİK BİLGİSİ
        // ====================================================================

        /// <summary>
        /// Activity'nin benzersiz tanımlayıcısı
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Bu aktivitenin ait olduğu Lead'in ID'si (Foreign Key)
        /// </summary>
        [Required]
        [ForeignKey(nameof(Lead))]
        // FOREIGN KEY:
        //   - Leads tablosunun Id'sine referans
        //   - Null olamaz: Her Activity mutlaka bir Lead'e ait
        //   - Cascade delete: Lead silinirse Activity'leri de silinir
        //   - Performance: İndeks otomatik oluşturulur
        public int LeadId { get; set; }

        /// <summary>
        /// Activity'nin ait olduğu Lead (Navigation Property)
        /// </summary>
        [Required]
        [NotMapped]
        public virtual Lead Lead { get; set; } = null!;
        // null!: Non-nullable reference types (C# 8.0+)
        //   - null! = "Compiler, bu null olmayacak, güven bana"
        //   - Runtime'da null ise NullReferenceException fırlatır
        //   - Avantaj: Null safety ama performance cost yok
        // virtual: Lazy loading için gerekli
        //   - EF Core proxy'si oluşturabilir

        // ====================================================================
        // AKTİVİTE TANIMLAMASI (Activity Definition)
        // ====================================================================

        /// <summary>
        /// Aktivitenin türü (Arama, Email, Toplantı, Görev vb.)
        /// </summary>
        [Required]
        [EnumDataType(typeof(ActivityType))]
        public ActivityType Type { get; set; }
        // ENUM VS STRING:
        //   ✓ Enum: Type-safe, SQL'de int saklanır, hızlı sorgu
        //   ✗ String: Flexible ama typo riski, veri kütlesinde
        //   Sonuç: Enum seçildi çünkü fixed category'ler

        /// <summary>
        /// Aktivitenin başlığı/özeti
        /// </summary>
        [Required]
        [StringLength(500, MinimumLength = 5, 
            ErrorMessage = "Başlık 5-500 karakter arasında olmalıdır.")]
        // BAŞLIK UZUNLUĞU:
        //   - Min 5: "Aç.." gibi anlamsız başlıkları engelle
        //   - Max 500: Reasonable limit (bir satırda görünemez)
        //   - Avantaj: Database consistency
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Aktivitenin detaylı açıklaması/notları
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        // nvarchar(max): Sınırsız text
        //   - Kullanıcı notları: Arama transkripsiyonu, toplantı notları
        //   - Avantaj: Esnek format
        //   - Dezavantaj: Full-text search zor, indeks yok
        public string? Description { get; set; };

        /// <summary>
        /// Aktivitenin durumu (Planlanmış, Tamamlanmış, İptal vb.)
        /// </summary>
        [Required]
        [EnumDataType(typeof(ActivityStatus))]
        public ActivityStatus Status { get; set; } = ActivityStatus.Scheduled;
        // DEFAULT: Scheduled
        //   - Yeni aktiviteler "Planlanmış" olarak başlar
        //   - User'ın completed'a değiştirmesi gerekir
        //   - Complete olana kadar workflow devam eder

        // ====================================================================
        // ZAMANSAL BİLGİLER (Temporal Information)
        // ====================================================================

        /// <summary>
        /// Aktivitenin planlandığı/başlaması gereken tarih/saat
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;
        // DEFAULT: Şu an (DateTime.UtcNow)
        //   - Aktivite oluşturulduğu anda planlanmış kabul edilir
        //   - User isterse değiştirebilir
        //   - Reminder'lar buna göre ayarlanır

        /// <summary>
        /// Aktivitenin tamamlanma tarih/saati (Null = Henüz tamamlanmamış)
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? CompletedAt { get; set; };
        // NULLABLE:
        //   - Null = Aktivite henüz tamamlanmamış
        //   - Completed'a değiştirilince otomatik set edilir
        //   - Avantaj: "Kaç aktivite hala pending?" sorusu kolay

        /// <summary>
        /// Aktivite sonrası takip tarihi (Reminder, follow-up scheduling)
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime? FollowUpScheduledAt { get; set; };
        // FOLLOW-UP SCHEDULING:
        //   - "3 gün sonra aramamı hatırla" gibi use case
        //   - Nullable: Her aktivitenin follow-up'u olmayabilir
        //   - Workflow: Aktivite tamamlandıktan sonra bu tarih active
        //   - Avantaj: Otomatik reminders oluşturulabilir

        /// <summary>
        /// Aktivitenin süresi (dakika cinsinden)
        /// </summary>
        [Range(0, 480, ErrorMessage = "Aktivite süresi 0-480 dakika arasında olmalıdır.")]
        // RANGE:
        //   - Min 0: Süresi 0 da olabilir (email, instant note)
        //   - Max 480: 8 saat (iş günü)
        //   - Avantaj: Reasonable limits
        //   - Avantaj: Kullanıcı hataları engellenir
        public int? DurationInMinutes { get; set; };
        // NULLABLE INT:
        //   - Bazı aktivitelerin süresi bilinmeyebilir
        //   - Email: Genelde süre tracking yok
        //   - Meeting: Süresi bellidir
        //   - Avantaj: Opsiyonel field

        // ====================================================================
        // SONUÇ VE AÇILIŞ BİLGİLERİ (Outcome Information)
        // ====================================================================

        /// <summary>
        /// Aktivitenin sonucu (Başarılı, Başarısız, İncineltici vb.)
        /// </summary>
        [EnumDataType(typeof(ActivityOutcome))]
        public ActivityOutcome? Outcome { get; set; };
        // NULLABLE ENUM:
        //   - Null = Henüz tamamlanmamış/sonuç bilinmiyor
        //   - Dolu = Tamamlandı ve sonuç kaydedildi
        //   - "Başarılı arama" vs "Aramaları cevaplamadı"
        //   - Avantaj: Veri analizi ve success rate hesabı

        /// <summary>
        /// Sonucun detaylı açıklaması
        /// </summary>
        [StringLength(1000)]
        public string? OutcomeDescription { get; set; };
        // DETAYLI SONUÇ:
        //   - "Müşteri Haziran'da satın almaya karar verecek"
        //   - "Hâlâ muhasebeci ile görüşmek istiyor"
        //   - "Rakibi seçmiş"
        //   - Avantaj: Neden başarısız olmuş bilmek

        // ====================================================================
        // KATILIMCI BİLGİLERİ (Participant Information)
        // ====================================================================

        /// <summary>
        /// Aktiviteyi oluşturan/sorumlu kişinin ID'si
        /// </summary>
        [Required]
        [StringLength(128)]
        [ForeignKey(nameof(AssignedToUser))]
        public string AssignedToUserId { get; set; } = string.Empty;
        // ASSIGNED TO:
        //   - Kim bu aktiviteyi yaptı/yapacak?
        //   - Accountability: Kimin sorumlulugu
        //   - Performance: Kimlerin kaç aktivitesi var
        //   - Avantaj: Workload tracking

        /// <summary>
        /// Aktivitenin atandığı kullanıcı (Navigation)
        /// </summary>
        [NotMapped]
        public virtual User? AssignedToUser { get; set; };
        // NAVIGATION PROPERTY:
        //   - User bilgisini lazy load edebiliriz
        //   - virtual: Proxy için gerekli
        //   - nullable: User silinebilir (orphaned)

        /// <summary>
        /// Aktiviteye katılan diğer kişiler (JSON array)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? Participants { get; set; };
        // PARTICIPANTS JSON:
        //   - [{"id":"user1","name":"Ali","email":"ali@example.com"}]
        //   - Nullable: Bazı aktivitelerde tek kişi olabilir
        //   - Avantaj: Flexible, migration free
        //   - Dezavantaj: Query etmek zor (Full table scan)
        //   - Optimization: Frequently queried ise ayrı table yapı

        // ====================================================================
        // AUDIT TRACK (Muhasebe/Denetim İzi)
        // ====================================================================

        /// <summary>
        /// Aktivitenin oluşturulma tarihi
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // UTC NOW:
        //   - Zamanı tutarlı tutmak için UTC kullan
        //   - Timezone issues yok
        //   - Immutable: Hiçbir zaman değişmez

        /// <summary>
        /// Aktivitenin son güncellenme tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; };
        // NULLABLE:
        //   - Null = Hiçbir zaman güncellenmemiş
        //   - Dolu = Güncelleme tarihi
        //   - Avantaj: Veri freshness tracking

        /// <summary>
        /// Aktiviteyi oluşturan kullanıcı
        /// </summary>
        [Required]
        [StringLength(128)]
        public string CreatedBy { get; set; } = "System";
        // CREATED BY:
        //   - Audit trail: Kim oluşturdu
        //   - Default: "System" (API call vs)
        //   - Immutable: Hiçbir zaman değişmez
    }

    // ========================================================================
    // ENUM TANIMLARI
    // ========================================================================

    /// <summary>
    /// Aktivite türleri
    /// </summary>
    public enum ActivityType
    {
        // ACTIVITY TYPES:
        //   - CRM'de yapılan etkileşimlerin kategorileri
        //   - Raporlama: "Bu ay kaç arama yapıldı?"
        //   - Workflow: Her tür için farklı template

        /// <summary>Telefon görüşmesi</summary>
        Call = 0,
        // CALL:
        //   - DurationInMinutes: Genelde doldurulur
        //   - Outcome: "Müşteri ilgilendi", "Kapalı arama"
        //   - Follow-up: Genelde var

        /// <summary>Email gönderimi</summary>
        Email = 1,
        // EMAIL:
        //   - Description: Email body
        //   - Outcome: "Açıldı", "Yanıt geldi"
        //   - Duration: Genelde 0 veya NULL

        /// <summary>Yüz yüze toplantı</summary>
        Meeting = 2,
        // MEETING:
        //   - ScheduledAt: Toplantı saati
        //   - CompletedAt: Bittimi bitince
        //   - Duration: Gerçek süre
        //   - Participants: Kimler katıldı

        /// <summary>Video konferans</summary>
        VideoCall = 3,
        // VIDEO CALL:
        //   - Remote: Ofiste olmayan toplantılar
        //   - Duration: Gerçek süre
        //   - Recording: Kaydedilmiş mi?

        /// <summary>Yapılacak görev/reminder</summary>
        Task = 4,
        // TASK:
        //   - FollowUpScheduledAt: Hatırlatma zamanı
        //   - Outcome: "Yapıldı", "Yapılmadı"
        //   - Internal: Müşteri görmez

        /// <summary>Sosyal medya etkileşimi</summary>
        SocialMedia = 5,
        // SOCIAL MEDIA:
        //   - LinkedIn message
        //   - Twitter mention
        //   - Facebook comment

        /// <summary>Konferans/Webinar katılımı</summary>
        EventAttendance = 6,
        // EVENT:
        //   - Ticari fuar
        //   - Webinar
        //   - Workshop

        /// <summary>Diğer etkileşim türleri</summary>
        Other = 7
    }

    /// <summary>
    /// Aktivite durumu
    /// </summary>
    public enum ActivityStatus
    {
        // STATUS WORKFLOW:
        //   - Scheduled -> Completed (Normal flow)
        //   - Scheduled -> Cancelled (Plan değişti)
        //   - Completed -> Rescheduled (Follow-up needed)

        /// <summary>Yapılması planlanmış, henüz yapılmamış</summary>
        Scheduled = 0,
        // DEFAULT: Yeni aktiviteler bu durumda
        //   - Reminder: Bu durumda olan aktiviteler için
        //   - Workflow: User tamamlayıncaya kadar beklenir

        /// <summary>Tamamlandı/yapıldı</summary>
        Completed = 1,
        // COMPLETED:
        //   - CompletedAt: Otomatik set edilir
        //   - Outcome: Mutlaka doldurulmalı
        //   - Historical: Raporlama için kullan

        /// <summary>İptal edildi/yapılmayacak</summary>
        Cancelled = 2,
        // CANCELLED:
        //   - Müşteri erişilemez
        //   - Plan değişti
        //   - Reason: Description'da

        /// <summary>Rescheduled - yeniden zamanlandı</summary>
        Rescheduled = 3
        // RESCHEDULED:
        //   - ScheduledAt: Yeni zamana güncellenir
        //   - Eski aktivite: Archived olabilir
    }

    /// <summary>
    /// Aktivite sonuçları
    /// </summary>
    public enum ActivityOutcome
    {
        // OUTCOME TRACKING:
        //   - Sales metric: Success rate hesabı
        //   - Forecasting: Kaç % likelihood kazanmak
        //   - Analysis: Hangi sonuç türü en çok?

        /// <summary>Başarılı - müşteri pozitif cevap verdi</summary>
        Successful = 0,n        // POZİTİF:
        //   - "Evet, satın alıyoruz"
        //   - "Teklifle ilgileniyorum"
        //   - Lead: Bir sonraki aşamaya geçer

        /// <summary>Başarısız - müşteri ilgilenmedi/olumsuz cevap</summary>
        Unsuccessful = 1,
        // NEGATİF:
        //   - "Hayır, ihtiyacımız yok"
        //   - "Başka bir satıcı seçtik"
        //   - Lead: Lost olabilir

        /// <summary>İncineltici - müşterideki sorun/olumsuzluk</summary>
        Objection = 2,
        // OBJECTION:
        //   - "Fiyat çok yüksek"
        //   - "Ürün özelliklerine sahip değil"
        //   - "Zamanında teslimat yok"
        //   - Follow-up: Objection çözülmeli

        /// <summary>Bilgi isteği - daha fazla detay istedi</summary>
        MoreInformationNeeded = 3,
        // MÜŞTERİ:
        //   - "Daha fazla detay gönderin"
        //   - "Case study istiyor"
        //   - "Demo görmek ister"
        //   - Follow-up: Gerekli materyalleri gönder

        /// <summary>Müşteriye erişilemedi</summary>
        NoAnswer = 4,
        // UNREACHABLE:
        //   - Telefon cevaplamadı
        //   - Email'e cevap yok
        //   - Retry: Daha sonra tekrar dene

        /// <summary>Kapalı arama - müşteri konuşmak istemedi</summary>
        NotInterested = 5
        // UNINTERESTED:
        //   - "Şu anda zamanım yok"
        //   - "Bunu kontrol edeceğim"
        //   - Lead: Soğuk (cold) olabilir, DND list'e ekle
    }
}
