using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class BankTransactionConfiguration : IEntityTypeConfiguration<BankTransaction>
    {
        public void Configure(EntityTypeBuilder<BankTransaction> builder)
        {
            builder.ToTable("BankTransactions");
            builder.HasKey(bt => bt.Id);

            builder.Property(bt => bt.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(bt => bt.Description)
                .HasMaxLength(500);

            builder.Property(bt => bt.Amount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(bt => bt.Currency)
                .IsRequired()
                .HasMaxLength(3);

            builder.Property(bt => bt.DebitCredit)
                .IsRequired()
                .HasMaxLength(1);

            builder.Property(bt => bt.BalanceAfterTransaction)
                .HasColumnType("decimal(18,2)");

            builder.Property(bt => bt.ReferenceNumber)
                .HasMaxLength(50);

            builder.Property(bt => bt.MatchedClCardCode)
                .HasMaxLength(50);

            builder.Property(bt => bt.MatchedClCardName)
                .HasMaxLength(255);

            builder.Property(bt => bt.TransferResult)
                .HasMaxLength(1000);

            builder.Property(bt => bt.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(bt => bt.ImportDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(bt => new { bt.UserId, bt.ImportDate });
            builder.HasIndex(bt => new { bt.BankId, bt.AccountNumber });
            builder.HasIndex(bt => bt.TransactionDate);
            builder.HasIndex(bt => bt.IsMatched);
            builder.HasIndex(bt => bt.IsTransferred);

            // Relationships
            builder.HasOne(bt => bt.Bank)
                .WithMany(b => b.BankTransactions)
                .HasForeignKey(bt => bt.BankId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(bt => bt.User)
                .WithMany(u => u.BankTransactions)
                .HasForeignKey(bt => bt.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(bt => bt.MatchedByUser)
                .WithMany()
                .HasForeignKey(bt => bt.MatchedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(bt => bt.TransferredByUser)
                .WithMany()
                .HasForeignKey(bt => bt.TransferredByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}