using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Business.Base;
using TurkSoft.Entities.BankService.Models;

namespace TurkSoft.Service.Interface
{
    public interface IBankStatementService
    {
        Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default);
    }
}
