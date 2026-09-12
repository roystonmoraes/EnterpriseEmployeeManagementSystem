using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Domain.Entities;
using EnterpriseEmployeeManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagementSystem.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly ApplicationDbContext _context;

    public DepartmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Departments.AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Department?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Departments.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }

    public async Task<Department?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Departments.FirstOrDefaultAsync(
            d => d.Name == name,
            cancellationToken
        );
    }

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default)
    {
        await _context.Departments.AddAsync(department, cancellationToken);
    }

    public void Remove(Department department)
    {
        _context.Departments.Remove(department);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> HasEmployeesAsync(
        int departmentId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Employees.AnyAsync(
            e => e.DepartmentId == departmentId,
            cancellationToken
        );
    }
}
