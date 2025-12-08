using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class UserVerificationDevice : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Cihaz adı (örn: iPhone 15, Chrome Tarayıcı)</summary>
        public string DeviceName { get; set; }

        /// <summary>Cihaz tipi (MobileApp, Sms, Email, Authenticator, WebBrowser...)</summary>
        public string DeviceType { get; set; }

        /// <summary>Cihaz tanımlayıcı (ör: device token, browser fingerprint vb.)</summary>
        public string DeviceIdentifier { get; set; }

        /// <summary>OTP / TOTP için secret (opsiyonel)</summary>
        public string SecretKey { get; set; }

        /// <summary>Varsayılan doğrulama cihazı mı?</summary>
        public bool IsPrimary { get; set; }

        /// <summary>Bu cihaz güvenilir olarak işaretlenmiş mi?</summary>
        public bool IsTrusted { get; set; }

        /// <summary>Son kullanım zamanı</summary>
        public DateTimeOffset? LastUsedAt { get; set; }
    }
}
