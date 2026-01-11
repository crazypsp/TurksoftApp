using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TurkSoft.Entities.Entities;

namespace TurkSoft.Data.EntityData
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.UserName)
                .IsUnique();

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.FirstName)
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .HasMaxLength(50);

            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.Property(u => u.PasswordSalt)
                .IsRequired();

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            builder.Property(u => u.IsEmailVerified)
                .HasDefaultValue(false);

            builder.Property(u => u.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            // Relationships
            builder.HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.BankCredentials)
                .WithOne(bc => bc.User)
                .HasForeignKey(bc => bc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.BankTransactions)
                .WithOne(bt => bt.User)
                .HasForeignKey(bt => bt.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.TransactionImports)
                .WithOne(ti => ti.User)
                .HasForeignKey(ti => ti.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.MatchingLogs)
                .WithOne(ml => ml.User)
                .HasForeignKey(ml => ml.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.TransferLogs)
                .WithOne(tl => tl.User)
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.ExportLogs)
                .WithOne(el => el.User)
                .HasForeignKey(el => el.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.SystemLogs)
                .WithOne(sl => sl.User)
                .HasForeignKey(sl => sl.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}