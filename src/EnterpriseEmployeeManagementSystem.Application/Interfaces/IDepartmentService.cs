using EnterpriseEmployeeManagementSystem.Application.DTOs.Departments;

namespace EnterpriseEmployeeManagementSystem.Application.Interfaces;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentResponse>> GetAllAsync(
        CancellationToken cancellationToken = default
    );

    Task<DepartmentResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<DepartmentResponse> CreateAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken = default
    );

    Task<DepartmentResponse> UpdateAsync(
        int id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default
    );

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
