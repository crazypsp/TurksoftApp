using TurkSoft.Business.Base;
using TurkSoft.Business.Interface;
using TurkSoft.Entities.BankService.Models;
using TurkSoft.Service.Interface;

namespace TurkSoft.Service.Manager
{
    public sealed class BankStatementService : IBankStatementService
    {
        private readonly IBankStatementManager _bankStatementManager;

        public BankStatementService(IBankStatementManager bankStatementManager)
        {
            _bankStatementManager = bankStatementManager;
        }

        public async Task<IReadOnlyList<BNKHAR>> GetStatementAsync(BankStatementRequest request, CancellationToken ct = default)
        {
            var list = await _bankStatementManager.GetStatementAsync(request, ct);
            return list; // List<BNKHAR> -> IReadOnlyList<BNKHAR> OK
        }
    }
}
