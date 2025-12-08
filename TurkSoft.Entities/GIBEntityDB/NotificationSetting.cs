using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class NotificationSetting : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Bildirim adı (örn: Fatura Hatası, Fatura Başarılı)</summary>
        public string Name { get; set; }

        /// <summary>Olay anahtarı (örn: INVOICE_ERROR, INVOICE_SUCCESS)</summary>
        public string EventKey { get; set; }

        /// <summary>Mail ile gönderilsin mi?</summary>
        public bool SendEmail { get; set; }

        /// <summary>SMS ile gönderilsin mi?</summary>
        public bool SendSms { get; set; }

        /// <summary>Push / uygulama bildirimi gönderilsin mi?</summary>
        public bool SendPush { get; set; }

        /// <summary>Webhook vs. için URL (opsiyonel)</summary>
        public string WebhookUrl { get; set; }

        /// <summary>Bildirim alıcıları (virgül ayrılmış mail / telefon / rol vb.)</summary>
        public string Targets { get; set; }

        /// <summary>Mail / bildirim konu şablonu</summary>
        public string SubjectTemplate { get; set; }

        /// <summary>Mail / bildirim gövde şablonu</summary>
        public string BodyTemplate { get; set; }
    }
}
