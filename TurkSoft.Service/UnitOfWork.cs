using System;
using System.Threading.Tasks;
using TurkSoft.Data;
using TurkSoft.Data.Context;
using TurkSoft.Entities.Entities;
using TurkSoft.Services.Interfaces;
using TurkSoft.Services.Repositories;

namespace TurkSoft.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        private IRepository<Bank> _bankRepository;
        public IRepository<Bank> BankRepository => _bankRepository ??= new Repository<Bank>(_context);

        private IRepository<BankAccount> _bankAccountRepository;
        public IRepository<BankAccount> BankAccountRepository => _bankAccountRepository ??= new Repository<BankAccount>(_context);

        private IRepository<BankCredential> _bankCredentialRepository;
        public IRepository<BankCredential> BankCredentialRepository => _bankCredentialRepository ??= new Repository<BankCredential>(_context);

        private IRepository<BankTransaction> _bankTransactionRepository;
        public IRepository<BankTransaction> BankTransactionRepository => _bankTransactionRepository ??= new Repository<BankTransaction>(_context);

        private IRepository<ExportLog> _exportLogRepository;
        public IRepository<ExportLog> ExportLogRepository => _exportLogRepository ??= new Repository<ExportLog>(_context);

        private IRepository<MatchingLog> _matchingLogRepository;
        public IRepository<MatchingLog> MatchingLogRepository => _matchingLogRepository ??= new Repository<MatchingLog>(_context);

        private IRepository<Role> _roleRepository;
        public IRepository<Role> RoleRepository => _roleRepository ??= new Repository<Role>(_context);

        private IRepository<SystemLog> _systemLogRepository;
        public IRepository<SystemLog> SystemLogRepository => _systemLogRepository ??= new Repository<SystemLog>(_context);

        private IRepository<TransactionImport> _transactionImportRepository;
        public IRepository<TransactionImport> TransactionImportRepository => _transactionImportRepository ??= new Repository<TransactionImport>(_context);

        private IRepository<TransferLog> _transferLogRepository;
        public IRepository<TransferLog> TransferLogRepository => _transferLogRepository ??= new Repository<TransferLog>(_context);

        private IRepository<User> _userRepository;
        public IRepository<User> UserRepository => _userRepository ??= new Repository<User>(_context);

        private IRepository<UserRole> _userRoleRepository;
        public IRepository<UserRole> UserRoleRepository => _userRoleRepository ??= new Repository<UserRole>(_context);

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public void Dispose() => _context?.Dispose();
    }
}