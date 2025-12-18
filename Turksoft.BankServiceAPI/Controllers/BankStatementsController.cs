using Microsoft.AspNetCore.Mvc;
using TurkSoft.Business.Base;
using TurkSoft.Entities.BankService.Models;
using TurkSoft.Service.Interface;

namespace Turksoft.BankServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankStatementsController : ControllerBase
    {
        private readonly IBankStatementService _bankStatementService;

        public BankStatementsController(IBankStatementService bankStatementService)
        {
            _bankStatementService = bankStatementService;
        }

        [HttpPost("statement")]
        public async Task<ActionResult<IReadOnlyList<BNKHAR>>> GetStatement(
            [FromBody] BankStatementRequest request,
            CancellationToken ct)
        {
            var result = await _bankStatementService.GetStatementAsync(request, ct);
            return Ok(result);
        }
    }
}
