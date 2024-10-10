using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;

namespace StudentProjectsCenterSystem.Infrastructure.Configurations
{
    public class ProjectDetailsSectionConfiguration : IEntityTypeConfiguration<ProjectDetailsSection>
    {
        public void Configure(EntityTypeBuilder<ProjectDetailsSection> builder)
        {
            builder
                .HasMany(p => p.ProjectDetails)
                .WithOne(pd => pd.ProjectDetailsSection)
                .HasForeignKey(pd => pd.ProjectDetailsSectionId);
        }
    }
}
