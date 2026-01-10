using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.ServiceEntity.Models
{
    public enum TransactionStatus
    {
        Beklemede = 0,
        Eslesitirildi = 1,
        Aktarildi = 2,
        Hata = 3
    }
}
