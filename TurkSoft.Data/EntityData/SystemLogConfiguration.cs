using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
    {
        public void Configure(EntityTypeBuilder<SystemLog> builder)
        {
            builder.ToTable("SystemLogs");
            builder.HasKey(sl => sl.Id);

            builder.Property(sl => sl.LogLevel)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(sl => sl.Message)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(sl => sl.Source)
                .HasMaxLength(500);

            builder.Property(sl => sl.IpAddress)
                .HasMaxLength(45);

            builder.Property(sl => sl.ActionName)
                .HasMaxLength(100);

            builder.Property(sl => sl.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationship
            builder.HasOne(sl => sl.User)
                .WithMany(u => u.SystemLogs)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}