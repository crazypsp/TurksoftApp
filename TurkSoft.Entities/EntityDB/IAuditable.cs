using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TurkSoft.Entities.EntityDB
{
    // Audit için (SaveChanges içinde doldurulur)
    public interface IAuditable
    {
        Guid CreatedByUserId { get; set; }
        Guid UpdatedByUserId { get; set; }
    }
}
