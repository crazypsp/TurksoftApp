using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.GIBEntityDB
{
    public class ParameterSetting : BaseEntity
    {
        public long Id { get; set; }

        /// <summary>Parametre kodu (örn: FaturaVarsayilanKur)</summary>
        public string Code { get; set; }

        /// <summary>Parametre adı</summary>
        public string Name { get; set; }

        /// <summary>Parametre değeri</summary>
        public string Value { get; set; }

        /// <summary>Veri tipi (String, Int, Decimal, Bool, DateTime...)</summary>
        public string DataType { get; set; }

        /// <summary>Gruplama (örn: Fatura, Sistem)</summary>
        public string Group { get; set; }
    }
}
