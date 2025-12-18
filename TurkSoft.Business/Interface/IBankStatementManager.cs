using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Base;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.Business.Interface
{
    public interface IBankStatementManager
    {
        Task<List<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default);
    }
}
