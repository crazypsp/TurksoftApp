using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class BankConfiguration : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {
            builder.ToTable("Banks");
            builder.HasKey(b => b.Id);

            builder.Property(b => b.ExternalBankId)
                .IsRequired();

            builder.HasIndex(b => b.ExternalBankId)
                .IsUnique();

            builder.Property(b => b.BankName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(b => b.Provider)
                .HasMaxLength(100);

            builder.Property(b => b.UsernameLabel)
                .HasMaxLength(100);

            builder.Property(b => b.PasswordLabel)
                .HasMaxLength(100);

            builder.Property(b => b.DefaultLink)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(b => b.DefaultTLink)
                .HasMaxLength(500)
                .IsRequired(false);

            builder.Property(b => b.IsActive)
                .HasDefaultValue(true);

            builder.Property(b => b.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasMany(b => b.BankAccounts)
                .WithOne(ba => ba.Bank)
                .HasForeignKey(ba => ba.BankId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.BankCredentials)
                .WithOne(bc => bc.Bank)
                .HasForeignKey(bc => bc.BankId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(b => b.BankTransactions)
                .WithOne(bt => bt.Bank)
                .HasForeignKey(bt => bt.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(b => b.TransactionImports)
                .WithOne(ti => ti.Bank)
                .HasForeignKey(ti => ti.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(b => b.MatchingLogs)
                .WithOne(ml => ml.Bank)
                .HasForeignKey(ml => ml.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}