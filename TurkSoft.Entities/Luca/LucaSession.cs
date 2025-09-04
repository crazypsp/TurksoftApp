using Microsoft.Playwright;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TurkSoft.Entities.Luca
{
    public static class LucaSession
    {
        /// <summary>
        /// Login sonrası açılan ana popup sayfası. RAM'de tutulur.
        /// </summary>
        public static IPage MmpPage { get; set; }
        public static IFrame Frame { get; set; }
        /// <summary>
        /// Hesap planı verileri bellekte tutulur, tekrar siteye gitmeye gerek kalmaz.
        /// </summary>
        public static List<CompanyCode> CachedFirma { get; set; } = new();
        /// <summary>
        /// Hesap planı verileri bellekte tutulur, tekrar siteye gitmeye gerek kalmaz.
        /// </summary>
        public static List<AccountingCode> CachedHesapPlani { get; set; } = new();
    }
}
