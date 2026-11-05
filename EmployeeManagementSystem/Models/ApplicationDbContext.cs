using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // SQLite cannot sort decimal columns, so the demo database stores salary as a number.
            if (Database.IsSqlite())
                modelBuilder.Entity<Employee>().Property(e => e.Salary).HasConversion<double>();
        }
    }
}
