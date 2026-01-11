using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class MatchingLogConfiguration : IEntityTypeConfiguration<MatchingLog>
    {
        public void Configure(EntityTypeBuilder<MatchingLog> builder)
        {
            builder.ToTable("MatchingLogs");
            builder.HasKey(ml => ml.Id);

            builder.Property(ml => ml.AccountNumber)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(ml => ml.Status)
                .HasMaxLength(20);

            builder.Property(ml => ml.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(ml => ml.MatchingCriteria)
                .HasMaxLength(4000); // JSON

            builder.Property(ml => ml.StartedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasOne(ml => ml.User)
                .WithMany(u => u.MatchingLogs)
                .HasForeignKey(ml => ml.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ml => ml.Bank)
                .WithMany(b => b.MatchingLogs)
                .HasForeignKey(ml => ml.BankId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}