using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TurkSoft.Core.Result.Interface;
using TurkSoft.Entities.Luca;

namespace TurkSoft.Business.Interface
{
    public interface ILucaAutomationBussiness
    {
        Task<IResult> LoginAsync(LucaLoginRequest request);
        Task<IDataResult<List<AccountingCode>>> GetAccountingPlanAsync();
        Task<IResult> SendFisRowsAsync(List<LucaFisRow> rows);
    }
}
