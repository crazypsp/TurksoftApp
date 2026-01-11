using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder.ToTable("BankAccounts");
            builder.HasKey(ba => ba.Id);

            builder.Property(ba => ba.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ba => ba.Currency)
                .IsRequired()
                .HasMaxLength(3);

            // ✅ BU ÜÇ SATIRI DEĞİŞTİRİN: IsRequired(false) ekleyin
            builder.Property(ba => ba.IBAN)
                .HasMaxLength(34)
                .IsRequired(false);

            builder.Property(ba => ba.SubeNo)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(ba => ba.MusteriNo)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(ba => ba.IsActive)
                .HasDefaultValue(true);

            // Composite unique index for BankId and AccountNumber
            builder.HasIndex(ba => new { ba.BankId, ba.AccountNumber })
                .IsUnique();

            // Relationship
            builder.HasOne(ba => ba.Bank)
                .WithMany(b => b.BankAccounts)
                .HasForeignKey(ba => ba.BankId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}