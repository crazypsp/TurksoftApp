using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class TransactionImportConfiguration : IEntityTypeConfiguration<TransactionImport>
    {
        public void Configure(EntityTypeBuilder<TransactionImport> builder)
        {
            builder.ToTable("TransactionImports");
            builder.HasKey(ti => ti.Id);

            builder.Property(ti => ti.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ti => ti.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(ti => ti.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(ti => ti.RequestParameters)
                .HasMaxLength(4000); // JSON

            builder.Property(ti => ti.StartedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(ti => ti.User)
                .WithMany(u => u.TransactionImports)
                .HasForeignKey(ti => ti.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ti => ti.Bank)
                .WithMany(b => b.TransactionImports)
                .HasForeignKey(ti => ti.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}