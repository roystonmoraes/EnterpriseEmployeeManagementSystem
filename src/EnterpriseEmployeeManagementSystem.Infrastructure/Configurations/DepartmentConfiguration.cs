using EnterpriseEmployeeManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseEmployeeManagementSystem.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasData(
            new Department
            {
                Id = 1,
                Name = "Engineering"
            },
            new Department
            {
                Id = 2,
                Name = "Human Resources"
            },
            new Department
            {
                Id = 3,
                Name = "Finance"
            },
            new Department
            {
                Id = 4,
                Name = "Sales"
            },
            new Department
            {
                Id = 5,
                Name = "IT"
            });
    }
}