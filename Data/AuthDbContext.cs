using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HR.Data
{
    public class AuthDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<JobRole> JobRoles { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<Employee_Project> Employee_Projects { get; set; }
        public DbSet<Employee_Training> Employee_Trainings { get; set; }
        public DbSet<JobPosting> JobPostings { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Grievance> Grievances { get; set; }
        public DbSet<Policy> Policies { get; set; }
        public DbSet<WellnessProgram> WellnessPrograms { get; set; }
        public DbSet<MentalHealthResource> MentalHealthResources { get; set; }
        public DbSet<PerformanceReview> PerformanceReviews { get; set; }
        public DbSet<DEIResource> DEIResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Department-Manager (Employee) relationship
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerID)
                .OnDelete(DeleteBehavior.Restrict);

            // Department-Employee relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.Department_ID);

            // Employee-JobRole relationship
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.JobRole)
                .WithMany()
                .HasForeignKey(e => e.JobRole_ID);

            // Payroll-Employee relationship (1:1)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Payroll)
                .WithOne(p => p.Employee)
                .HasForeignKey<Payroll>(p => p.Employee_ID);

            // Attendance-Employee relationship
            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Attendances)
                .HasForeignKey(a => a.Employee_ID);

            // Leave-Employee relationship
            modelBuilder.Entity<Leave>()
                .HasOne(l => l.Employee)
                .WithMany(e => e.Leaves)
                .HasForeignKey(l => l.Employee_ID);

            // Project-Employee_Project relationship
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Employee_Projects)
                .WithOne(ep => ep.Project)
                .HasForeignKey(ep => ep.Project_ID);

            // Training-Employee_Training relationship
            modelBuilder.Entity<Training>()
                .HasMany(t => t.Employee_Trainings)
                .WithOne(et => et.Training)
                .HasForeignKey(et => et.Training_ID);

            // Employee_Project (many-to-many Employee-Project)
            modelBuilder.Entity<Employee_Project>()
                .HasOne(ep => ep.Employee)
                .WithMany(e => e.Employee_Projects)
                .HasForeignKey(ep => ep.Employee_ID);

            // Employee_Training (many-to-many Employee-Training)
            modelBuilder.Entity<Employee_Training>()
                .HasOne(et => et.Employee)
                .WithMany(e => e.Employee_Trainings)
                .HasForeignKey(et => et.Employee_ID);

            // Application-JobPosting relationship
            modelBuilder.Entity<Application>()
                .HasOne(a => a.JobPosting)
                .WithMany()
                .HasForeignKey(a => a.JobPosting_ID);
            // Grievance-Employee relationship
            modelBuilder.Entity<Grievance>()
                .HasOne(g => g.Employee)
                .WithMany()
                .HasForeignKey(g => g.Employee_ID);
            // PerformanceReview-Employee relationship
            modelBuilder.Entity<PerformanceReview>()
                .HasOne(pr => pr.Employee)
                .WithMany()
                .HasForeignKey(pr => pr.Employee_ID);
        }
    }
}
