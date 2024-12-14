using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentProjectsCenter.Core.Entities.Domain.Terms;

namespace StudentProjectsCenterSystem.Infrastructure.Configurations
{
    public class TermsConfiguration : IEntityTypeConfiguration<TermGroup>
    {
        public void Configure(EntityTypeBuilder<TermGroup> builder)
        {
            // Configure the one-to-many relationship
            builder
                .HasMany(tg => tg.Terms)
                .WithOne(t => t.TermGroup)
                .HasForeignKey(t => t.TermGroupId);

        }
    }
}
