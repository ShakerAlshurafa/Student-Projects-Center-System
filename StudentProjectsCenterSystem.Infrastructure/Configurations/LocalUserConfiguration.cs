using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentProjectsCenterSystem.Core.Entities;

namespace StudentProjectsCenterSystem.Infrastructure.Configurations
{
    public class LocalUserConfiguration : IEntityTypeConfiguration<LocalUser>
    {
        public void Configure(EntityTypeBuilder<LocalUser> builder)
        {
            // FirstName
            builder.Property(r => r.FirstName)
                   .IsRequired()
                   .HasMaxLength(50);

            // MiddleName
            builder.Property(r => r.MiddleName)
                   .HasMaxLength(50);

            // LastName
            builder.Property(r => r.LastName)
                   .IsRequired()
                   .HasMaxLength(50);

            // Email
            builder.Property(r => r.Email)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.HasIndex(r => r.Email).IsUnique(); // Ensure email uniqueness

            // CompanyName
            builder.Property(r => r.CompanyName)
                   .HasMaxLength(100);
        }
    }
}
