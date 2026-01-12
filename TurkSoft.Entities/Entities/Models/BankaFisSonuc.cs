using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.Entities.Models
{
    public class BankaFisSonuc
    {
        public bool Basarili { get; set; }
        public string Mesaj { get; set; }
        public int FisReferans { get; set; }
        public string IslemKodu { get; set; }
        public DateTime IslemTarihi { get; set; }
        public List<string> Hatalar { get; set; } = new List<string>();
        public int LogoFisNo { get; set; }
        public string GUID { get; set; }
    }

    public class KrediTaksitSonuc : BankaFisSonuc
    {
        public decimal OdenenAnaPara { get; set; }
        public decimal OdenenFaiz { get; set; }
        public decimal OdenenBsmv { get; set; }
        public decimal ToplamOdenen { get; set; }
        public int TaksitNo { get; set; }
    }

    public class IslemDurumuSonuc
    {
        public bool Basarili { get; set; }
        public string Mesaj { get; set; }
        public int FisReferans { get; set; }
        public string Durum { get; set; }
        public DateTime SorgulamaTarihi { get; set; }
    }
}
