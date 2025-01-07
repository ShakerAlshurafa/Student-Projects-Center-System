using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentProjectsCenter.Core.Entities.Domain.workgroup;

namespace StudentProjectsCenterSystem.Infrastructure.Configurations
{
    public class CelenderConfiguration : IEntityTypeConfiguration<Celender>
    {
        public void Configure(EntityTypeBuilder<Celender> builder)
        {
            builder
                .HasOne(c => c.Workgroup)
                .WithMany(w => w.CelenderEvents)
                .HasForeignKey(c => c.WorkgroupId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
