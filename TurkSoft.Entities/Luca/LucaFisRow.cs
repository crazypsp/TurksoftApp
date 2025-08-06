using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.Luca
{
    public class LucaFisRow
    {
        public string HesapKodu {  get; set; }
        public string EvrakNo { get; set; }
        public DateTime Tarih {  get; set; }
        public string Aciklama {  get; set; }
        public decimal Borc {  get; set; }
        public decimal Alacak { get; set; }
    }
}
