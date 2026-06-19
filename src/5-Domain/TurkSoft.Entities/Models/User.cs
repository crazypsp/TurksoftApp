using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ============================================================================
// DOMAIN MODEL: User (Kullanıcı - Sistem Kullanıcısı)
// ============================================================================
// 
// AMAÇ:
//   User sınıfı, CRM sisteminin kullanıcılarını temsil eder.
//   Satış temsilcileri, yöneticiler, sistem administratörleri vb.
//   AspNetCore Identity ile entegre edilebilir.
//
// SORUMLULUKLAR:
//   1. Kullanıcı bilgileri (ad, email, telefon)
//   2. Kullanıcı rolü ve izinleri
//   3. Kullanıcı durumu (aktif, inaktif, locked)
//   4. Kullanıcı performans metrikleri
//
// ============================================================================

namespace TurkSoft.Entities.Models
{
    /// <summary>
    /// User (Kullanıcı) Entity - CRM sistem kullanıcısı
    /// </summary>
    [Table("Users", Schema = "Security")]
    public class User
    {
        // ====================================================================
        // PRIMARY KEY
        // ====================================================================

        /// <summary>
        /// Kullanıcının benzersiz tanımlayıcısı
        /// </summary>
        [Key]
        [StringLength(128)]
        // GUID vs STRING:
        //   - String: AspNetCore Identity uyumlu (Id kullanır)
        //   - Avantaj: GUID yerine daha readable
        //   - Avantaj: Migration'ı kolay
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // ====================================================================
        // TEMELİ BİLGİLER
        // ====================================================================

        /// <summary>
        /// Kullanıcının adı ve soyadı
        /// </summary>
        [Required]
        [StringLength(200)]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Kullanıcının email adresi (Unique)
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;
        // UNIQUE CONSTRAINT:
        //   - EF Core: .HasIndex(u => u.Email).IsUnique()
        //   - SQL: UNIQUE INDEX
        //   - Avantaj: Email ile tek login

        /// <summary>
        /// Kullanıcının telefon numarası
        /// </summary>
        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; };

        // ====================================================================
        // KULLANICI ROLÜ VE YETKİ
        // ====================================================================

        /// <summary>
        /// Kullanıcının rolü (Admin, Manager, Sales, Viewer)
        /// </summary>
        [Required]
        [EnumDataType(typeof(UserRole))]
        public UserRole Role { get; set; } = UserRole.SalesRep;
        // ROLE:
        //   - Admin: Sistem yönetimi
        //   - Manager: Bölüm yöneticisi (reports görebilir)
        //   - SalesRep: Satış temsilcisi (kendi leads'i)
        //   - Viewer: Salt okunur (reporting only)

        /// <summary>
        /// Kullanıcı departmanı (Sales, Marketing, Management)
        /// </summary>
        [StringLength(100)]
        public string? Department { get; set; };

        /// <summary>
        /// Kullanıcının raporlanacağı manager'ı
        /// </summary>
        [StringLength(128)]
        [ForeignKey(nameof(ManagerUser))]
        public string? ManagerId { get; set; };
        // MANAGER HIERARCHY:
        //   - NULL = Top level (CEO/Director)
        //   - Dolu = Reporting manager
        //   - Avantaj: Organizational hierarchy

        /// <summary>
        /// Kullanıcının manager'ı (Navigation)
        /// </summary>
        [NotMapped]
        public virtual User? ManagerUser { get; set; };

        /// <summary>
        /// Bu kullanıcının subordinate'leri (Navigation)
        /// </summary>
        [NotMapped]
        public virtual ICollection<User> Subordinates { get; set; } 
            = new List<User>();

        // ====================================================================
        // KULLANICI DURUMU
        // ====================================================================

        /// <summary>
        /// Kullanıcı aktif mi?
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;
        // ACTIVE FLAG:
        //   - false = Kullanıcı deaktif (login yapamayor)
        //   - true = Kullanıcı aktif (normal)
        //   - Avantaj: Soft delete yerine deactivation
        //   - Avantaj: Daha sonra tekrar aktif edilebilir

        /// <summary>
        /// Kullanıcı hesabı kilitli mi (Çok fazla hatalı login)
        /// </summary>
        [Required]
        public bool IsLocked { get; set; } = false;
        // LOCK MECHANISM:
        //   - Security: Brute force attack koruması
        //   - Process: 5 hatalı login > locked
        //   - Admin: Unlock etmek için admin gerekli

        /// <summary>
        /// Hesabın kilitli olma nedeni
        /// </summary>
        [StringLength(500)]
        public string? LockReason { get; set; };

        /// <summary>
        /// Hesabın kilit kalkacağı tarih (Auto-unlock)
        /// </summary>
        public DateTime? LockedUntil { get; set; };
        // TEMPORARY LOCK:
        //   - DateTime.UtcNow.AddMinutes(30) = 30 dakika kilitli
        //   - Auto-unlock: Background job kontrol eder
        //   - Avantaj: Manual unlock gerek yok

        // ====================================================================
        // AUDİT TRACK
        // ====================================================================

        /// <summary>
        /// Kullanıcının oluşturulma tarihi
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Kullanıcının son güncellenme tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; };

        /// <summary>
        /// Kullanıcının son login tarihi
        /// </summary>
        public DateTime? LastLoginAt { get; set; };
        // LAST LOGIN:
        //   - NULL = Hiç login yapmamış
        //   - Dolu = En son login zamanı
        //   - Avantaj: Inactivity tracking
        //   - Report: "30 gündür login yapmayan users"

        /// <summary>
        /// Kullanıcının son IP adresi (Security audit)
        /// </summary>
        [StringLength(45)]
        // 45 CHARS:
        //   - IPv4: max 15 chars
        //   - IPv6: max 39 chars
        //   - Buffer: 45 chars yeterli
        public string? LastLoginIp { get; set; };

        // ====================================================================
        // İLİŞKİLER
        // ====================================================================

        /// <summary>
        /// Kullanıcıya atanmış tüm Lead'ler
        /// </summary>
        [NotMapped]
        public virtual ICollection<Lead> AssignedLeads { get; set; } 
            = new List<Lead>();
        // LEADS:
        //   - One-to-Many: 1 User -> N Leads
        //   - Navigation: User'ın tüm leads'ini getir
        //   - Query: User -> Leads efficient loading

        /// <summary>
        /// Kullanıcıya atanmış tüm Activity'ler
        /// </summary>
        [NotMapped]
        public virtual ICollection<Activity> AssignedActivities { get; set; } 
            = new List<Activity>();

        /// <summary>
        /// Kullanıcının oluşturduğu Note'lar
        /// </summary>
        [NotMapped]
        public virtual ICollection<Note> CreatedNotes { get; set; } 
            = new List<Note>();
    }

    // ========================================================================
    // ENUM TANIMLARI
    // ========================================================================

    /// <summary>
    /// Kullanıcı rolleri (Authorization)
    /// </summary>
    public enum UserRole
    {
        // ROLE HIERARCHY:
        //   - Admin: Tüm permission'lar
        //   - Manager: Ekip yönetimi + reporting
        //   - SalesRep: Kendi leads'i
        //   - Viewer: Salt okunur

        /// <summary>Sistem yöneticisi (Full access)</summary>
        Admin = 0,
        // ADMİN:
        //   - Permissions: Tüm işlemler
        //   - Users: Create/Edit/Delete
        //   - Settings: Sistem ayarları
        //   - Reporting: Tüm raporlar

        /// <summary>Satış müdürü/Yönetici (Bölüm yönetimi)</summary>
        Manager = 1,
        // MANAGER:
        //   - Permissions: Kendi ekibinin leads'i
        //   - Reports: Ekibin performansı
        //   - Approve: Önemli discountlar
        //   - Cannot: Users create/delete

        /// <summary>Satış temsilcisi (Temel erişim)</summary>
        SalesRep = 2,
        // SALES REP:
        //   - Permissions: Kendi leads'i
        //   - Reports: Sadece kendi reports
        //   - Limitations: Başka sales'in leads'ini göremez

        /// <summary>Viewer/Raportçu (Salt okunur)</summary>
        Viewer = 3
        // VIEWER:
        //   - Permissions: Create, Edit, Delete yok
        //   - Reports: Tüm raporlar (read-only)
        //   - Use case: Direktör, Board
    }
}
