using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.Document
{
    public class BankaHareket
    {
        public DateTime Tarih { get; set; }
        public string Aciklama { get; set; }
        public decimal Tutar { get; set; }
        public decimal? Bakiye { get; set; }
        public string HesapKodu { get; set; }
        public string KaynakDosya { get; set; }
        public string BankaAdi { get; set; }
        public string KlasorYolu { get; set; }
    }
}
