using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class BankCredentialConfiguration : IEntityTypeConfiguration<BankCredential>
    {
        public void Configure(EntityTypeBuilder<BankCredential> builder)
        {
            builder.ToTable("BankCredentials");
            builder.HasKey(bc => bc.Id);

            builder.Property(bc => bc.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(bc => bc.Password)
                .IsRequired()
                .HasMaxLength(500); // Encrypted

            builder.Property(bc => bc.Extras)
                .HasMaxLength(2000); // JSON

            builder.Property(bc => bc.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Composite unique index for UserId and BankId
            builder.HasIndex(bc => new { bc.UserId, bc.BankId })
                .IsUnique();

            // Relationships
            builder.HasOne(bc => bc.User)
                .WithMany(u => u.BankCredentials)
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(bc => bc.Bank)
                .WithMany(b => b.BankCredentials)
                .HasForeignKey(bc => bc.BankId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}