using System;
using System.Threading.Tasks;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;

namespace TurkSoft.Services
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Bank> BankRepository { get; }
        IRepository<BankAccount> BankAccountRepository { get; }
        IRepository<BankCredential> BankCredentialRepository { get; }
        IRepository<BankTransaction> BankTransactionRepository { get; }
        IRepository<ExportLog> ExportLogRepository { get; }
        IRepository<MatchingLog> MatchingLogRepository { get; }
        IRepository<Role> RoleRepository { get; }
        IRepository<SystemLog> SystemLogRepository { get; }
        IRepository<TransactionImport> TransactionImportRepository { get; }
        IRepository<TransferLog> TransferLogRepository { get; }
        IRepository<User> UserRepository { get; }
        IRepository<UserRole> UserRoleRepository { get; }

        Task<int> CommitAsync();
    }
}