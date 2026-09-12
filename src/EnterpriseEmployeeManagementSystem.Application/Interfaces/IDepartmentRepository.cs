using EnterpriseEmployeeManagementSystem.Domain.Entities;

namespace EnterpriseEmployeeManagementSystem.Application.Interfaces;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Department?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<Department?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<bool> HasEmployeesAsync(int departmentId, CancellationToken cancellationToken = default);

    Task AddAsync(Department department, CancellationToken cancellationToken = default);

    void Remove(Department department);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
