﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StudentProjectsCenterSystem.Core.Entities.project;

namespace StudentProjectsCenterSystem.Infrastructure.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder
                .HasMany(p => p.ProjectDetails)
                .WithOne(pd => pd.Project)
                .HasForeignKey(pd => pd.ProjectId);

        }
    }
}