using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class ExportLogConfiguration : IEntityTypeConfiguration<ExportLog>
    {
        public void Configure(EntityTypeBuilder<ExportLog> builder)
        {
            builder.ToTable("ExportLogs");
            builder.HasKey(el => el.Id);

            builder.Property(el => el.ExportType)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(el => el.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(el => el.FilterCriteria)
                .HasMaxLength(4000); // JSON

            builder.Property(el => el.FilePath)
                .HasMaxLength(500);

            builder.Property(el => el.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship
            builder.HasOne(el => el.User)
                .WithMany(u => u.ExportLogs)
                .HasForeignKey(el => el.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}