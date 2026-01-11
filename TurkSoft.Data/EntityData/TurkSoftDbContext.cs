using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class TurkSoftDbContext : DbContext
    {
        public TurkSoftDbContext(DbContextOptions<TurkSoftDbContext> options) : base(options)
        {
        }

        // User Management
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        // Bank Management
        public DbSet<Bank> Banks { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<BankCredential> BankCredentials { get; set; }

        // Transactions
        public DbSet<BankTransaction> BankTransactions { get; set; }
        public DbSet<TransactionImport> TransactionImports { get; set; }

        // Logs
        public DbSet<MatchingLog> MatchingLogs { get; set; }
        public DbSet<TransferLog> TransferLogs { get; set; }
        public DbSet<ExportLog> ExportLogs { get; set; }
        public DbSet<SystemLog> SystemLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the Configurations namespace
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
            modelBuilder.ApplyConfiguration(new BankConfiguration());
            modelBuilder.ApplyConfiguration(new BankAccountConfiguration());
            modelBuilder.ApplyConfiguration(new BankCredentialConfiguration());
            modelBuilder.ApplyConfiguration(new BankTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionImportConfiguration());
            modelBuilder.ApplyConfiguration(new MatchingLogConfiguration());
            modelBuilder.ApplyConfiguration(new TransferLogConfiguration());
            modelBuilder.ApplyConfiguration(new ExportLogConfiguration());
            modelBuilder.ApplyConfiguration(new SystemLogConfiguration());

            // Seed test data
            //EntityData.Seeding.TestSeedData.Apply(modelBuilder);
        }
    }
}