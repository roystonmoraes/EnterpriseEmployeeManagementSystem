using EnterpriseEmployeeManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagementSystem.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();
}