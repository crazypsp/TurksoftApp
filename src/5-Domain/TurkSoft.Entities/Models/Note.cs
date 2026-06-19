using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================
// DOMAIN MODEL: Note (Not - İç Notlar)
// ============================================================================
// 
// AMAÇ:
//   Note sınıfı, Lead hakkında yapılan internal notları tutar.
//   Satış ekibi üyeleri arasında haberleşme aracıdır.
//   Aktiviteden farkı: Note, internal comment'dir, external action değil.
//
// ÖRNEK NOTLAR:
//   - "Müşteri cuma günü karar verecek"
//   - "Teknik detayları sordu, datasheet gönderdim"
//   - "CEO'nun onayı gerekli, pazartesi sonucu alacak"
//   - "Fiyat konusunda %10 indirim talebi"
//
// ============================================================================

namespace TurkSoft.Entities.Models
{
    /// <summary>
    /// Note (Not) Entity - Lead hakkındaki internal notlar
    /// </summary>
    [Table("Notes", Schema = "CRM")]
    public class Note
    {
        // ====================================================================
        // PRIMARY KEY
        // ====================================================================

        /// <summary>
        /// Not'un benzersiz tanımlayıcısı
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Not'un ait olduğu Lead'in ID'si (Foreign Key)
        /// </summary>
        [Required]
        [ForeignKey(nameof(Lead))]
        public int LeadId { get; set; }

        /// <summary>
        /// Not'un ait olduğu Lead (Navigation Property)
        /// </summary>
        [NotMapped]
        public virtual Lead Lead { get; set; } = null!;

        // ====================================================================
        // NOT İÇERİĞİ
        // ====================================================================

        /// <summary>
        /// Not'un başlığı (İsteğe bağlı, detaylı notlar için)
        /// </summary>
        [StringLength(300)]
        public string? Title { get; set; };
        // BAŞLIK:
        //   - "Fiyat indirim talebi" gibi kısa özet
        //   - Nullable: Kısa notlar başlıksız olabilir
        //   - Avantaj: Not listesinde hızlı scan

        /// <summary>
        /// Not'un tam içeriği
        /// </summary>
        [Required]
        [StringLength(5000, MinimumLength = 1, 
            ErrorMessage = "Not 1-5000 karakter arasında olmalıdır.")]
        // CONTENT:
        //   - Min 1: En az 1 karakter gerekli
        //   - Max 5000: Makul limit (uzun not)
        //   - Avantaj: Spam/boş notlar engellenir
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Not'un türü (Internal comment vs Alert vs Decision)
        /// </summary>
        [EnumDataType(typeof(NoteType))]
        public NoteType Type { get; set; } = NoteType.Comment;
        // NOTE TYPE:
        //   - Comment: Rastgele not
        //   - Alert: Uyarı/hatırlatma (kırmızı flag)
        //   - Decision: Önemli karar
        //   - Action: Yapılması gereken işlem
        //   - Risk: Riskli durum

        /// <summary>
        /// Not'un görünürlük seviyesi (Public vs Private)
        /// </summary>
        [EnumDataType(typeof(NoteVisibility))]
        public NoteVisibility Visibility { get; set; } = NoteVisibility.Public;
        // VİSİBİLİTY:
        //   - Public: Tüm ekip görebilir
        //   - Private: Sadece oluşturan kişi ve yöneticiler
        //   - Confidential: Sadece lead owner
        //   - Avantaj: Gizlilik kontrol

        // ====================================================================
        // AUDIT TRACK
        // ====================================================================

        /// <summary>
        /// Not'u oluşturan kullanıcının ID'si
        /// </summary>
        [Required]
        [StringLength(128)]
        [ForeignKey(nameof(CreatedByUser))]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Not'u oluşturan kullanıcı (Navigation)
        /// </summary>
        [NotMapped]
        public virtual User? CreatedByUser { get; set; };

        /// <summary>
        /// Not'un oluşturulma tarihi
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Not'un güncellenme tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; };

        /// <summary>
        /// Not'u güncelleyen kullanıcının ID'si
        /// </summary>
        [StringLength(128)]
        public string? UpdatedBy { get; set; };

        // ====================================================================
        // ENGAGEMENT
        // ====================================================================

        /// <summary>
        /// Not'a yapılan "beğeni" sayısı (Engagement metric)
        /// </summary>
        [Range(0, int.MaxValue)]
        public int LikeCount { get; set; } = 0;
        // LİKE COUNT:
        //   - Helpful not'lar daha çok beğeni alır
        //   - Performance: Valuable notes bulunabilir
        //   - Social proof: "Diğer kişiler de yararlı bulmuş"

        /// <summary>
        /// Not'a yapılan reply'ler (Thread)
        /// </summary>
        [NotMapped]
        public virtual ICollection<NoteReply> Replies { get; set; } 
            = new List<NoteReply>();
        // REPLİES:
        //   - Thread başlığı gibi davranır
        //   - Collaboration: Tartışma yönetilebilir
        //   - Notification: Reply'ler ekiple bildirilir
    }

    /// <summary>
    /// NoteReply (Not Cevabı) - Not'a yapılan reply'ler
    /// </summary>
    [Table("NoteReplies", Schema = "CRM")]
    public class NoteReply
    {
        /// <summary>
        /// Reply'nin benzersiz tanımlayıcısı
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Hangi Not'a reply'dir
        /// </summary>
        [Required]
        [ForeignKey(nameof(Note))]
        public int NoteId { get; set; }

        /// <summary>
        /// İlişkili Not (Navigation)
        /// </summary>
        [NotMapped]
        public virtual Note Note { get; set; } = null!;

        /// <summary>
        /// Reply'nin içeriği
        /// </summary>
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Reply'yi yapan kullanıcı
        /// </summary>
        [Required]
        [StringLength(128)]
        [ForeignKey(nameof(CreatedByUser))]
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Reply'yi yapan kullanıcı (Navigation)
        /// </summary>
        [NotMapped]
        public virtual User? CreatedByUser { get; set; };

        /// <summary>
        /// Reply'nin oluşturulma tarihi
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // ========================================================================
    // ENUM TANIMLARI
    // ========================================================================

    /// <summary>
    /// Not türleri
    /// </summary>
    public enum NoteType
    {
        /// <summary>Rastgele not/yorum</summary>
        Comment = 0,
        // AÇIKLAMALAR:
        //   - En yaygın not türü
        //   - Tema: Rastgele bilgiler

        /// <summary>Uyarı/dikkat edilmesi gereken konu</summary>
        Alert = 1,
        // UYARI:
        //   - "Müşteri ses tonundan memnun değilmiş"
        //   - "Bütçe sıkıntıları olabilir"
        //   - UI: Kırmızı ikon/badge

        /// <summary>Önemli karar/anlaşma</summary>
        Decision = 2,
        // KARAR:
        //   - "Müşteri pazartesi satın almaya karar verdi"
        //   - "Fiyat konusunda anlaşmaya vardık"
        //   - UI: Yeşil/öne çıkınız badge

        /// <summary>Yapılması gereken aksiyon</summary>
        Action = 3,
        // İŞLEM:
        //   - "Teknik spec göndermem gerekiyor"
        //   - "CEO ile toplantı ayarla"
        //   - Workflow: Task create et

        /// <summary>Risk/problem bildirimi</summary>
        Risk = 4,
        // RİSK:
        //   - "Rakip aktif satış yapıyor"
        //   - "Müşteri tekrar iletişime geçmedi"
        //   - Management: Dikkat gerekli

        /// <summary>İçgörü/analiz</summary>
        Insight = 5
        // İÇGÖRÜ:
        //   - "Bu müşteri pattern'i şuna benziyor"
        //   - "Tarihsel olarak bu dönemde satın alıyor"
        //   - Knowledge sharing
    }

    /// <summary>
    /// Not görünürlüğü/gizlilik seviyesi
    /// </summary>
    public enum NoteVisibility
    {
        /// <summary>Herkese görünür</summary>
        Public = 0,

        /// <summary>Sadece lead owner ve managers</summary>
        Private = 1,

        /// <summary>Sadece lead owner</summary>
        Confidential = 2
    }
}
