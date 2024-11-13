using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentProjectsCenterSystem.Core.Entities;
using StudentProjectsCenterSystem.Core.Entities.Domain.project;
using StudentProjectsCenterSystem.Core.Entities.Domain.workgroup;
using StudentProjectsCenterSystem.Core.Entities.project;


namespace StudentProjectsCenterSystem.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<LocalUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(Configurations.ProjectConfiguration).Assembly);
            builder.ApplyConfigurationsFromAssembly(typeof(Configurations.ProjectDetailsSectionConfiguration).Assembly);
            builder.ApplyConfigurationsFromAssembly(typeof(Configurations.UserProjectConfiguration).Assembly);
            base.OnModelCreating(builder);
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectDetailEntity> ProjectDetails { get; set; }
        public DbSet<ProjectDetailsSection> ProjectDetailsSections { get; set; }
        public DbSet<LocalUser> LocalUsers { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<Workgroup> Workgroups { get; set; }
        public DbSet<StudentProjectsCenterSystem.Core.Entities.Domain.workgroup.Task> Tasks { get; set; }

    }
}
