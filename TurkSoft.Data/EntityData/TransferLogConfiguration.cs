using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class TransferLogConfiguration : IEntityTypeConfiguration<TransferLog>
    {
        public void Configure(EntityTypeBuilder<TransferLog> builder)
        {
            builder.ToTable("TransferLogs");
            builder.HasKey(tl => tl.Id);

            builder.Property(tl => tl.TransferType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(tl => tl.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(tl => tl.TargetSystem)
                .HasMaxLength(50);

            builder.Property(tl => tl.RequestData)
                .HasMaxLength(4000); // JSON

            builder.Property(tl => tl.ResponseData)
                .HasMaxLength(4000); // JSON

            builder.Property(tl => tl.ErrorMessage)
                .HasMaxLength(1000);

            builder.Property(tl => tl.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship
            builder.HasOne(tl => tl.User)
                .WithMany(u => u.TransferLogs)
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}