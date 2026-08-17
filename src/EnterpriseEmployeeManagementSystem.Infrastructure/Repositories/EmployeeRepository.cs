using EnterpriseEmployeeManagementSystem.Application.Interfaces;
using EnterpriseEmployeeManagementSystem.Domain.Entities;
using EnterpriseEmployeeManagementSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseEmployeeManagementSystem.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AnyAsync(e => e.Email == email, cancellationToken);
    }

    public async Task AddAsync(
        Employee employee,
        CancellationToken cancellationToken = default)
    {
        await _context.Employees.AddAsync(employee, cancellationToken);
    }

    public void Update(Employee employee)
    {
        _context.Employees.Update(employee);
    }

    public void Delete(Employee employee)
    {
        _context.Employees.Remove(employee);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPagedAsync(
    string? search,
    bool? activeOnly,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default)
{
    var query = _context.Employees
        .Include(e => e.Department)
        .AsNoTracking()
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        search = search.Trim();

        query = query.Where(e =>
            e.FirstName.Contains(search) ||
            e.LastName.Contains(search) ||
            e.Email.Contains(search));
    }

    if (activeOnly.HasValue)
    {
        query = query.Where(e =>
            e.IsActive == activeOnly.Value);
    }

    var totalCount = await query.CountAsync(
        cancellationToken);

    var items = await query
        .OrderBy(e => e.LastName)
        .ThenBy(e => e.FirstName)
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync(cancellationToken);

    return (items, totalCount);
}
}