using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Infrastructure.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder
                .HasMany(p => p.ProjectDetailsSection)
                .WithOne(pd => pd.Project)
                .HasForeignKey(pd => pd.ProjectId);

            builder
                .HasOne(p => p.Workgroup)
                .WithOne(w => w.Project)
                .HasForeignKey<Project>(p => p.WorkgroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
