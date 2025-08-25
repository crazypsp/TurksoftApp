using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    public enum BildirimKanal { Email = 0, Sms = 1, Push = 2, Web = 3 }
    public enum BildirimDurum { Olusacak = 0, Gonderildi = 1, Hata = 2 }
    public class SistemBildirim:BaseEntity
    {
        public BildirimKanal Kanal { get; set; }
        public DateTime? PlanlananTarih { get; set; }
        public string Baslik { get; set; }
        public string Icerik { get; set; }
        public BildirimDurum Durum { get; set; }
        public Guid? HedefKullaniciId { get; set; }
    }
}
