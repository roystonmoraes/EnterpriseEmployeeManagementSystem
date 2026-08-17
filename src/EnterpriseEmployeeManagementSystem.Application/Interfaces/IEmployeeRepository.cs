using EnterpriseEmployeeManagementSystem.Domain.Entities;

namespace EnterpriseEmployeeManagementSystem.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Employee>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Employee employee,
        CancellationToken cancellationToken = default);

    void Update(Employee employee);

    void Delete(Employee employee);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Employee> Items, int TotalCount)> GetPagedAsync(
    string? search,
    bool? activeOnly,
    int pageNumber,
    int pageSize,
    CancellationToken cancellationToken = default);
}