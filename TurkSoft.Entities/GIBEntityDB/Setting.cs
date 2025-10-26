﻿using System;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class Setting
    {
        public long Id { get; set; }

        // 🔧 Firma Bilgileri
        public string CompanyName { get; set; }
        public string VknTckn { get; set; }
        public string Address { get; set; }

        // 👤 Kullanıcı Bilgileri
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // ⚙️ Genel Ayarlar
        public string Theme { get; set; }
        public string Language { get; set; }

        // 📅 Metadata
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
